﻿using System;
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
using Oqtane.Modules;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Repository;
using Microsoft.AspNetCore.Http;

namespace Oqtane.ChatHubs.Hubs
{

    [AllowAnonymous]
    public class ChatHub : Hub
    {
        private IHttpContextAccessor httpContextAccessor;
        private readonly IChatHubRepository chatHubRepository;
        private readonly IChatHubService chatHubService;
        private readonly IUserRepository userRepository;
        private readonly UserManager<IdentityUser> identityUserManager;
        private readonly IRoleRepository roles;
        private readonly IUserRoleRepository userRoles;

        public ChatHub(
            IHttpContextAccessor httpContextAccessor,
            IChatHubRepository chatHubRepository,
            IChatHubService chatHubService,
            IUserRepository userRepository,
            UserManager<IdentityUser> identityUserManager,
            IRoleRepository roles,
            IUserRoleRepository userRoles
            )
        {
            this.httpContextAccessor = httpContextAccessor;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.userRepository = userRepository;
            this.identityUserManager = identityUserManager;
            this.roles = roles;
            this.userRoles = userRoles;
        }

        [AllowAnonymous]
        public override async Task OnConnectedAsync()
        {
            var httpContext = this.httpContextAccessor.HttpContext;

            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            List<ChatHubRoom> list = this.chatHubRepository.GetChatHubRooms(int.Parse(moduleId)).ToList();

            string platform = Context.GetHttpContext().Request.Headers["platform"];

            string guestname = null;
            guestname = Context.GetHttpContext().Request.Query["guestname"];
            guestname = guestname.Trim();

            if (!string.IsNullOrEmpty(guestname) && !this.IsValidGuestUsername(guestname))
            {
                throw new HubException("No valid username.");
            }

            string username = this.CreateGuestUsername(guestname);
            string email = "invalidemail@oqtane.com";
            string password = "$D33F4ULT";

            /*
            IdentityUser identityuser = await this.identityUserManager.FindByNameAsync(username);
            if (identityuser == null)
            {
                identityuser = new IdentityUser();
                identityuser.UserName = username;
                identityuser.Email = email;
                identityuser.EmailConfirmed = false;
                var result = await this.identityUserManager.CreateAsync(identityuser, password);
                if (result.Succeeded) { }
            }
            */

            ChatHubUser chatHubUser = new ChatHubUser()
            {
                SiteId = 1,
                Username = username,
                DisplayName = guestname,
                Email = email,
                LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
            };
            chatHubUser = this.chatHubRepository.AddChatHubUser(chatHubUser);

            if (chatHubUser != null && chatHubUser.Username != Constants.HostUser)
            {
                // add auto assigned roles to user for site
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
                Status = (int)ChatHubConnectionStatus.Active
            };
            ChatHubConnection = this.chatHubRepository.AddChatHubConnection(ChatHubConnection);

            ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(chatHubUser);

            await Clients.Client(Context.ConnectionId).SendAsync("OnConnected", chatHubUserClientModel);
            await base.OnConnectedAsync();
        }
        [AllowAnonymous]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            if (guest != null)
            {
                foreach(var connection in guest.Connections)
                {
                    connection.Status = (int)ChatHubConnectionStatus.Inactive;
                    chatHubRepository.UpdateChatHubConnection(connection);
                }
                
                var rooms = chatHubRepository.GetChatHubRoomsByUser(guest).Active();
                foreach (var room in rooms.Where(room => room.Type == (int)ChatHubRoomType.Public))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.ChatHubRoomId.ToString());                    
                    if(guest.Connections.Active().Count() <= 1)
                    {
                        var chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(guest);
                        await Clients.Group(room.ChatHubRoomId.ToString()).SendAsync("RemoveUser", chatHubUserClientModel, room.ChatHubRoomId.ToString());
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        [AllowAnonymous]
        public async Task EnterChatRoom(int roomId, int moduleId)
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            if (guest != null)
            {

                ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);
                ChatHubRoomChatHubUser room_user = new ChatHubRoomChatHubUser()
                {
                    ChatHubRoomId = room.ChatHubRoomId,
                    ChatHubUserId = guest.UserId
                };
                chatHubRepository.AddChatHubRoomChatHubUser(room_user);

                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in guest.Connections.Active())
                {
                    await Groups.AddToGroupAsync(connection.ConnectionId, room.ChatHubRoomId.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("AddRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(guest);
                await Clients.Group(room.ChatHubRoomId.ToString()).SendAsync("AddUser", chatHubUserClientModel, room.ChatHubRoomId.ToString());
            }
        }
        [AllowAnonymous]
        public async Task LeaveChatRoom(int roomId, int moduleId)
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            if (guest != null)
            {

                this.chatHubRepository.DeleteChatHubRoomChatHubUser(roomId, guest.UserId);
                ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);
                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in guest.Connections.Active())
                {
                    await Groups.RemoveFromGroupAsync(connection.ConnectionId, room.ChatHubRoomId.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("RemoveRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(guest);
                await Clients.Group(room.ChatHubRoomId.ToString()).SendAsync("RemoveUser", chatHubUserClientModel, room.ChatHubRoomId.ToString());
            }
        }

        [AllowAnonymous]
        public async Task SendMessage(string content, int roomId, int moduleId)
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            if (guest != null)
            {

                ChatHubMessage chatHubMessage = new ChatHubMessage()
                {
                    ChatHubRoomId = roomId,
                    ChatHubUserId = guest.UserId,
                    User = guest,
                    Content = content ?? string.Empty,
                    Type = (int)ChatHubMessageType.Guest
                };
                this.chatHubRepository.AddChatHubMessage(chatHubMessage);

                ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
                var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(guest);
                await Clients.GroupExcept(roomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);

            }
        }

        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredUsers()
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            IQueryable<ChatHubUser> ignoredUsers = null;
            if (guest != null)
            {
                ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(guest);
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
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            IQueryable<ChatHubIgnore> ignoredByUsers = null;
            if (guest != null)
            {
                ignoredByUsers = this.chatHubRepository.GetIgnoredByUsers(guest);
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
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (guest != null && targetUser != null)
            {
                if (guest == targetUser)
                {
                    throw new HubException("Calling user cannot be target user.");
                }

                ChatHubIgnore chatHubIgnore = null;
                var users = this.chatHubRepository.GetIgnoredUsers(targetUser);
                chatHubIgnore = users.Where(u => u.ChatHubUserId == targetUser.UserId).FirstOrDefault();

                if (chatHubIgnore != null)
                {
                    chatHubIgnore.ModifiedOn = DateTime.Now;
                    this.chatHubRepository.UpdateChatHubIgnore(chatHubIgnore);
                }
                else
                {
                    chatHubIgnore = new ChatHubIgnore()
                    {
                        ChatHubUserId = guest.UserId,
                        ChatHubIgnoredUserId = targetUser.UserId,
                        User = guest
                    };

                    this.chatHubRepository.AddChatHubIgnore(chatHubIgnore);
                }

                var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                foreach (var connection in guest.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredUser", targetUserClientModel);
                }

                var userClientModel = this.chatHubService.CreateChatHubUserClientModel(guest);
                foreach (var connection in targetUser.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredByUser", userClientModel);
                }
            }
        }
        [AllowAnonymous]
        public async Task UnignoreUser(string username)
        {
            ChatHubUser guest = await this.chatHubService.IdentifyGuest(Context.ConnectionId);
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (guest != null && targetUser != null)
            {
                var chatHubIgnores = await this.chatHubRepository.GetIgnoredUsers(guest).ToListAsync();
                ChatHubIgnore chatHubIgnore = chatHubIgnores.FirstOrDefault(item => item.ChatHubIgnoredUserId == targetUser.UserId);

                if (chatHubIgnore != null)
                {
                    this.chatHubRepository.DeleteChatHubIgnore(chatHubIgnore);

                    var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                    foreach (var connection in guest.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredUser", targetUserClientModel);
                    }

                    var userClientModel = this.chatHubService.CreateChatHubUserClientModel(guest);
                    foreach (var connection in targetUser.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredByUser", userClientModel);
                    }
                }
            }
        }

        private string CreateGuestUsername(string guestName)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string id = Regex.Replace(base64Guid, "[/+=]", "");
            string userName = string.Concat(guestName, "-", new Random().Next(6000, 6999), "-", id);

            return userName;
        }

        private bool IsValidGuestUsername(string guestName)
        {
            string guestNamePattern = "^([a-zA-Z0-9_]{3,32})$";
            Regex regex = new Regex(guestNamePattern);
            Match match = regex.Match(guestName);
            return match.Success;
        }

    }
}