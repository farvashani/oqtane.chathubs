using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using System.Text.RegularExpressions;
using Oqtane.Shared.Enums;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Oqtane.Shared;
using Microsoft.EntityFrameworkCore;
using Oqtane.Shared.Models;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.ChatHubs.Commands;

namespace Oqtane.ChatHubs.Hubs
{

    [AllowAnonymous]
    public class ChatHub : Hub
    {
        private IHttpContextAccessor httpContextAccessor;
        private readonly IChatHubRepository chatHubRepository;
        private readonly IChatHubService chatHubService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRoleRepository roles;
        private readonly IUserRoleRepository userRoles;

        public ChatHub(
            IHttpContextAccessor httpContextAccessor,
            IChatHubRepository chatHubRepository,
            IChatHubService chatHubService,
            UserManager<IdentityUser> identityUserManager,
            IRoleRepository roles,
            IUserRoleRepository userRoles
            )
        {
            this.httpContextAccessor = httpContextAccessor;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.userManager = identityUserManager;
            this.roles = roles;
            this.userRoles = userRoles;
        }

        private async Task<ChatHubUser> OnConnectedGuest()
        {
            string guestname = null;
            guestname = Context.GetHttpContext().Request.Query["guestname"];
            guestname = guestname.Trim();

            if (!string.IsNullOrEmpty(guestname) && !this.IsValidGuestUsername(guestname))
            {
                throw new HubException("No valid username.");
            }

            string username = this.CreateUsername(guestname);
            string displayname = this.CreateDisplaynameFromUsername(username);

            if (await this.chatHubRepository.GetUserByDisplayName(displayname) != null)
            {
                throw new HubException("Displayname already in use. Goodbye.");
            }

            string email = "noreply@chathub.tv";
            string password = "§PasswordPolicy42";

            ChatHubUser chatHubUser = new ChatHubUser()
            {
                SiteId = 1,
                Username = username,
                DisplayName = displayname,
                Email = email,
                LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
            };
            chatHubUser = this.chatHubRepository.AddChatHubUser(chatHubUser);

            if (chatHubUser != null && chatHubUser.Username != Constants.HostUser)
            {
                List<Role> roles = this.roles.GetRoles(chatHubUser.SiteId).Where(item => item.IsAutoAssigned).ToList();
                foreach (Role role in roles)
                {
                    UserRole userrole = new UserRole();
                    userrole.UserId = chatHubUser.UserId;
                    userrole.RoleId = role.RoleId;
                    userrole.EffectiveDate = null;
                    userrole.ExpiryDate = null;
                    userRoles.AddUserRole(userrole);
                }
            }

            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
            };
            ChatHubConnection = this.chatHubRepository.AddChatHubConnection(ChatHubConnection);

            ChatHubSetting ChatHubSetting = new ChatHubSetting()
            {
                UsernameColor = "#7744aa",
                MessageColor = "#44aa77",
                ChatHubUserId = chatHubUser.UserId
            };
            ChatHubSetting = this.chatHubRepository.AddChatHubSetting(ChatHubSetting);

            return chatHubUser;
        }
        private async Task<ChatHubUser> OnConnectedUser(ChatHubUser chatHubUser)
        {
            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
            };
            ChatHubConnection = this.chatHubRepository.AddChatHubConnection(ChatHubConnection);

            ChatHubSetting ChatHubSetting = this.chatHubRepository.GetChatHubSettingByUser(chatHubUser);
            if(ChatHubSetting == null)
            {
                ChatHubSetting = new ChatHubSetting()
                {
                    UsernameColor = "#7744aa",
                    MessageColor = "#44aa77",
                    ChatHubUserId = chatHubUser.UserId
                };
                ChatHubSetting = this.chatHubRepository.AddChatHubSetting(ChatHubSetting);
            }

            return chatHubUser;
        }
        [AllowAnonymous]
        public override async Task OnConnectedAsync()
        {
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            List<ChatHubRoom> list = this.chatHubRepository.GetChatHubRooms(int.Parse(moduleId)).ToList();

            string platform = Context.GetHttpContext().Request.Headers["platform"];

            ChatHubUser user = await this.chatHubService.IdentifyUser(Context);
            if (user != null)
            {
                user = await this.OnConnectedUser(user);
            }
            else
            {
                user = await this.OnConnectedGuest();
            }

            ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
            await Clients.Client(Context.ConnectionId).SendAsync("OnConnected", chatHubUserClientModel);
            await base.OnConnectedAsync();
        }
        [AllowAnonymous]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            var rooms = chatHubRepository.GetChatHubRoomsByUser(user).Active();
            foreach (var room in await rooms.ToListAsync())
            {
                if (user.Connections.Active().Count() == 1)
                {
                    var chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                    await Clients.Group(room.Id.ToString()).SendAsync("RemoveUser", chatHubUserClientModel, room.Id.ToString());
                }

                await this.SendGroupNotification(string.Format("{0} disconnected from chat with client device {1}.", user.DisplayName, this.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Connect_Disconnect);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());
            }

            var connection = await this.chatHubRepository.GetConnectionByConnectionId(Context.ConnectionId);
            connection.Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Inactive);
            chatHubRepository.UpdateChatHubConnection(connection);

            await base.OnDisconnectedAsync(exception);
        }

        [AllowAnonymous]
        public async Task EnterChatRoom(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);
            if (this.chatHubRepository.GetChatHubUsersByRoom(room).Any(item => item.UserId == user.UserId))
            {
                throw new HubException("User already entered room.");
            }

            if (room.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(room, user))
                {
                    throw new HubException("No valid one vs one room id.");
                }
            }

            if (room.Public() || room.OneVsOne())
            {
                ChatHubRoomChatHubUser room_user = new ChatHubRoomChatHubUser()
                {
                    ChatHubRoomId = room.Id,
                    ChatHubUserId = user.UserId
                };
                chatHubRepository.AddChatHubRoomChatHubUser(room_user);

                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in user.Connections.Active())
                {
                    await Groups.AddToGroupAsync(connection.ConnectionId, room.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("AddRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                await Clients.Group(room.Id.ToString()).SendAsync("AddUser", chatHubUserClientModel, room.Id.ToString());

                await this.SendGroupNotification(string.Format("{0} entered chat room with client device {1}.", user.DisplayName, this.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Enter_Leave);
            }
        }
        [AllowAnonymous]
        public async Task LeaveChatRoom(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);
            if (!this.chatHubRepository.GetChatHubUsersByRoom(room).Any(item => item.UserId == user.UserId))
            {
                throw new HubException("User already left room.");
            }

            if (room.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(room, user))
                {
                    throw new HubException("No valid one vs one room id.");
                }
            }

            if (room.Public() || room.OneVsOne())
            {
                this.chatHubRepository.DeleteChatHubRoomChatHubUser(roomId, user.UserId);
                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in user.Connections.Active())
                {
                    await Groups.RemoveFromGroupAsync(connection.ConnectionId, room.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("RemoveRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                await Clients.Group(room.Id.ToString()).SendAsync("RemoveUser", chatHubUserClientModel, room.Id.ToString());
                await this.SendGroupNotification(string.Format("{0} left chat room with client device {1}.", user.DisplayName, this.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Enter_Leave);
            }
        }

        private async Task<bool> ExecuteCommandManager(ChatHubUser chatHubUser, string message, int roomId)
        {
            var commandManager = new CommandManager(chatHubUser.UserId, Context.ConnectionId, roomId, this, chatHubService, chatHubRepository, userManager);
            return await commandManager.TryHandleCommand(message);
        }

        [AllowAnonymous]
        public async Task SendCommandMetaDatas(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            var callerUserRole = Constants.AllUsersRole;
            List<ChatHubCommandMetaData> commandMetaDatas = CommandManager.GetCommandsMetaDataByUserRole(callerUserRole).ToList();

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = user.UserId,
                User = user,
                Content = string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Commands),
                CommandMetaDatas = commandMetaDatas
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            await Clients.Clients(user.Connections.Active().Select(c => c.ConnectionId).ToArray<string>()).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        [AllowAnonymous]
        public async Task SendMessage(string message, int roomId, int moduleId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            if (await ExecuteCommandManager(user, message, roomId))
            {
                return;
            }

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = user.UserId,
                User = user,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Guest)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(user);
            await Clients.GroupExcept(roomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        public async Task SendClientNotification(string message, int roomId, string connectionId, ChatHubUser targetUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = targetUser.UserId,
                User = targetUser,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            await Clients.Client(connectionId).SendAsync("AddMessage", chatHubMessageClientModel);
        }
        public async Task SendGroupNotification(string message, int roomId, string connectionId, ChatHubUser contextUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = contextUser.UserId,
                User = contextUser,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
            await Clients.GroupExcept(roomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredUsers()
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            IQueryable<ChatHubUser> ignoredUsers = null;
            if (user != null)
            {
                ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(user);
            }

            if (ignoredUsers != null && ignoredUsers.Any())
            {
                List<ChatHubUser> chatHubUserClientModel = new List<ChatHubUser>();
                foreach (var ignoredUser in ignoredUsers)
                {
                    ChatHubUser viewModel = this.chatHubService.CreateChatHubUserClientModel(ignoredUser);
                    chatHubUserClientModel.Add(viewModel);
                }

                return chatHubUserClientModel;
            }

            return null;
        }
        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredByUsers()
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            IQueryable<ChatHubIgnore> ignoredByUsers = null;
            if (user != null)
            {
                ignoredByUsers = this.chatHubRepository.GetIgnoredByUsers(user);
            }

            IQueryable<ChatHubUser> chatHubUserClientModels = ignoredByUsers.Select(x =>

                this.chatHubService.CreateChatHubUserClientModel(x.User)
            );

            var list = await chatHubUserClientModels.ToListAsync();
            return list;
        }

        [AllowAnonymous]
        public async Task IgnoreUser(string username)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (user != null && targetUser != null)
            {
                if (user == targetUser)
                {
                    throw new HubException("Calling user cannot be target user.");
                }

                this.chatHubService.IgnoreUser(user, targetUser);

                var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                foreach (var connection in user.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredUser", targetUserClientModel);
                }

                var userClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                foreach (var connection in targetUser.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredByUser", userClientModel);
                }
            }
        }
        [AllowAnonymous]
        public async Task UnignoreUser(string username)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (user != null && targetUser != null)
            {
                var chatHubIgnores = await this.chatHubRepository.GetIgnoredUsers(user).ToListAsync();
                ChatHubIgnore chatHubIgnore = chatHubIgnores.FirstOrDefault(item => item.ChatHubIgnoredUserId == targetUser.UserId);

                if (chatHubIgnore != null)
                {
                    this.chatHubRepository.DeleteChatHubIgnore(chatHubIgnore);

                    var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                    foreach (var connection in user.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredUser", targetUserClientModel);
                    }

                    var userClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                    foreach (var connection in targetUser.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredByUser", userClientModel);
                    }
                }
            }
        }

        private string CreateUsername(string guestname)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string id = Regex.Replace(base64Guid, "[/+=]", "");
            string userName = string.Concat(guestname, "-", new Random().Next(1000, 9999), "-", id);

            return userName;
        }
        private string CreateDisplaynameFromUsername(string username)
        {
            var name = username.Substring(0, username.IndexOf('-'));
            var numbers = username.Substring(username.IndexOf('-') + 1, 4);
            var displayname = string.Concat(name, "-", numbers);
            return displayname;
        }

        private string MakeStringAnonymous(string value, int tolerance, char symbol = '*')
        {
            if(tolerance >= value.Length)
            {
                return string.Empty;
            }

            var newValue = value.Substring(0, value.Length - tolerance);
            for (var i = 0; i <= tolerance; i++)
            {
                newValue += symbol;
            }

            return newValue;
        }

        private bool IsValidGuestUsername(string guestName)
        {
            string guestNamePattern = "^([a-zA-Z0-9_]{3,32})$";
            Regex regex = new Regex(guestNamePattern);
            Match match = regex.Match(guestName);
            return match.Success;
        }

        private async Task<ChatHubUser> GetChatHubUserAsync()
        {
            ChatHubUser user = await this.chatHubService.IdentifyUser(Context);
            if (user == null)
            {
                user = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            }

            if (user == null)
            {
                throw new HubException("No valid user found.");
            }

            return user;
        }

    }
}