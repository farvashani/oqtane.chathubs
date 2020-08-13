using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Oqtane.ChatHubs.Repository;
using Oqtane.Modules;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class ChatHubService : IChatHubService, IService
    {

        private readonly IChatHubRepository chatHubRepository;

        public ChatHubService(
            IChatHubRepository chatHubRepository
            )
        {
            this.chatHubRepository = chatHubRepository;
        }

        public async Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room)
        {
            List<ChatHubMessage> lastMessages = new List<ChatHubMessage>();
            if(room.OneVsOne())
            {
                lastMessages = await this.chatHubRepository.GetChatHubMessages(room.Id, new TimeSpan(24,0,0)).ToListAsync();
                lastMessages = lastMessages != null && lastMessages.Any() ? lastMessages.Select(item => this.CreateChatHubMessageClientModel(item)).ToList() : new List<ChatHubMessage>();
            }

            List<ChatHubUser> onlineUsers = await this.chatHubRepository.GetOnlineUsers(room.Id).ToListAsync();
            onlineUsers = onlineUsers != null && onlineUsers.Any() ? onlineUsers = onlineUsers.Select(item => this.CreateChatHubUserClientModel(item)).ToList() : new List<ChatHubUser>();

            return new ChatHubRoom()
            {
                Id = room.Id,
                ModuleId = room.ModuleId,
                Title = room.Title,
                Content = room.Content,
                ImageUrl = room.ImageUrl,
                Type = room.Type,
                Status = room.Status,
                Messages = lastMessages,
                Users = onlineUsers,
                CreatedOn = room.CreatedOn,
                CreatedBy = room.CreatedBy,
                ModifiedBy = room.ModifiedBy,
                ModifiedOn = room.ModifiedOn
            };
        }

        public ChatHubUser CreateChatHubUserClientModel(ChatHubUser user)
        {
            IEnumerable<ChatHubConnection> activeConnections = user.Connections.Active();

            ChatHubSetting chatHubSettings = this.chatHubRepository.GetChatHubSetting(user.UserId);
            ChatHubSetting chatHubSettingClientModel = this.CreateChatHubSettingClientModel(chatHubSettings);

            return new ChatHubUser()
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Connections = activeConnections.Any() ? new List<ChatHubConnection>() : activeConnections.Select(item => CreateChatHubConnectionClientModel(item)).ToList(),
                Settings = chatHubSettingClientModel ?? null,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy
            };
        }

        public ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message)
        {
            List<ChatHubPhoto> photos = message.Photos != null && message.Photos.Any() ? message.Photos.Select(item => CreateChatHubPhotoClientModel(item)).ToList() : null;
            ChatHubUser user = message.User != null ? this.CreateChatHubUserClientModel(message.User) : null;

            return new ChatHubMessage()
            {
                Id = message.Id,
                ChatHubRoomId = message.ChatHubRoomId,
                ChatHubUserId = message.ChatHubUserId,
                User = user,
                Content = message.Content,
                Type = message.Type,
                Photos = photos,
                CommandMetaDatas = message.CommandMetaDatas,
                CreatedOn = message.CreatedOn,
                CreatedBy = message.CreatedBy,
                ModifiedOn = message.ModifiedOn,
                ModifiedBy = message.ModifiedBy
            };
        }

        public ChatHubConnection CreateChatHubConnectionClientModel(ChatHubConnection connection)
        {
            return new ChatHubConnection()
            {
                ChatHubUserId = connection.ChatHubUserId,
                Status = connection.Status,
                User = connection.User,
                CreatedOn = connection.CreatedOn,
                CreatedBy = connection.CreatedBy,
                ModifiedOn = connection.ModifiedOn,
                ModifiedBy = connection.ModifiedBy
            };
        }

        public ChatHubPhoto CreateChatHubPhotoClientModel(ChatHubPhoto photo)
        {
            return new ChatHubPhoto()
            {
                ChatHubMessageId = photo.ChatHubMessageId,
                Source = photo.Source,
                Thumb = photo.Thumb,
                Caption = photo.Caption,
                Size = photo.Size,
                Width = photo.Width,
                Height = photo.Height,
                CreatedOn = photo.CreatedOn,
                CreatedBy = photo.CreatedBy,
                ModifiedOn = photo.ModifiedOn,
                ModifiedBy = photo.ModifiedBy
            };
        }

        public ChatHubSetting CreateChatHubSettingClientModel(ChatHubSetting settings)
        {
            return new ChatHubSetting()
            {
                UsernameColor = settings.UsernameColor,
                MessageColor = settings.MessageColor,
                CreatedOn = settings.CreatedOn,
                CreatedBy = settings.CreatedBy,
                ModifiedOn = settings.ModifiedOn,
                ModifiedBy = settings.ModifiedBy
            };
        }

        public void IgnoreUser(ChatHubUser callerUser, ChatHubUser targetUser)
        {
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
                    ChatHubUserId = callerUser.UserId,
                    ChatHubIgnoredUserId = targetUser.UserId,
                    User = callerUser
                };

                this.chatHubRepository.AddChatHubIgnore(chatHubIgnore);
            }
        }

        public async Task<ChatHubUser> IdentifyGuest(string connectionId)
        {
            ChatHubConnection connection = await Task.Run(() => chatHubRepository.GetConnectionByConnectionId(connectionId));
            if (connection != null)
            {
                return await this.chatHubRepository.GetUserByIdAsync(connection.User.UserId);
            }

            return null;
        }
        public async Task<ChatHubUser> IdentifyUser(HubCallerContext Context)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                ChatHubUser user = await this.chatHubRepository.GetUserByUserNameAsync(Context.User.Identity.Name);
                return user;
            }

            return null;
        }

        public List<string> GetAllExceptConnectionIds(ChatHubUser user)
        {
            var list = new List<ChatHubUser>();

            var ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();
            var ignoredByUsers = this.chatHubRepository.GetIgnoredByApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();

            list.AddRange(ignoredUsers);
            list.AddRange(ignoredByUsers);

            List<string> connectionsIds = new List<string>();

            foreach (var item in list)
            {
                foreach (var connection in item.Connections.Active())
                {
                    connectionsIds.Add(connection.ConnectionId);
                }
            }

            return connectionsIds;
        }

        public ChatHubRoom GetOneVsOneRoom(ChatHubUser callerUser, ChatHubUser targetUser, int moduleId)
        {
            if (callerUser != null && targetUser != null)
            {
                var oneVsOneRoom = this.chatHubRepository.GetChatHubRoomOneVsOne(this.CreateOneVsOneId(callerUser, targetUser));
                if(oneVsOneRoom != null)
                {
                    return oneVsOneRoom;
                }

                ChatHubRoom chatHubRoom = new ChatHubRoom()
                {
                    ModuleId = moduleId,
                    Title = string.Format("{0} vs {1}", callerUser.DisplayName, targetUser.DisplayName),
                    Content = "One Vs One",
                    Type = ChatHubRoomType.OneVsOne.ToString(),
                    Status = ChatHubRoomStatus.Active.ToString(),
                    ImageUrl = string.Empty,
                    OneVsOneId = this.CreateOneVsOneId(callerUser, targetUser)
                };
                return this.chatHubRepository.AddChatHubRoom(chatHubRoom);
            }

            return null;
        }
        public string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2)
        {
            var list = new List<string>();
            list.Add(user1.UserId.ToString());
            list.Add(user2.UserId.ToString());
            list = list.OrderBy(item => item).ToList();
            string roomId = string.Concat(list.First(), "|", list.Last());

            return roomId;
        }
        public bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller)
        {
            return room.OneVsOneId.Split('|').OrderBy(item => item).Any(item => item == caller.UserId.ToString());
        }

    }
}
