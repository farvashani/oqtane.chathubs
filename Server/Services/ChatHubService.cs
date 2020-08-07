using Microsoft.EntityFrameworkCore;
using Oqtane.ChatHubs.Repository;
using Oqtane.Modules;
using Oqtane.Repository;
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
        private readonly IUserRepository userRepository;

        public ChatHubService(
            IChatHubRepository chatHubRepository,
            IUserRepository userRepository
            )
        {
            this.chatHubRepository = chatHubRepository;
            this.userRepository = userRepository;
        }

        public async Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room)
        {
            IList<ChatHubUser> onlineUsers = await this.chatHubRepository.GetOnlineUsers(room).ToListAsync();

            return new ChatHubRoom()
            {
                ChatHubRoomId = room.ChatHubRoomId,
                ModuleId = room.ModuleId,
                Title = room.Title,
                Content = room.Content,
                ImageUrl = room.ImageUrl,
                Type = room.Type,
                Status = room.Status,
                Messages = new List<ChatHubMessage>(),
                Users = !onlineUsers.Any() ? new List<ChatHubUser>() : onlineUsers.Select(x => this.CreateChatHubUserClientModel(x)).ToList(),
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
                Connections = activeConnections.Any() ? new List<ChatHubConnection>() : activeConnections.Select(x => new ChatHubConnection()
                {
                    ChatHubConnectionId = x.ChatHubConnectionId,
                    CreatedOn = x.CreatedOn
                }).ToList(),
                Settings = chatHubSettingClientModel ?? null,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy
            };
        }

        public ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message)
        {
            List<ChatHubPhoto> photos = message.Photos != null && message.Photos.Any() ? message.Photos.Select(x => CreateChatHubPhotoClientModel(x)).ToList() : null;

            return new ChatHubMessage()
            {
                ChatHubMessageId = message.ChatHubMessageId,
                ChatHubRoomId = message.ChatHubRoomId,
                ChatHubUserId = message.ChatHubUserId,
                User = this.CreateChatHubUserClientModel(message.User),
                Content = message.Content,
                Type = message.Type,
                Photos = photos,
                CommandMetaDatas = message.CommandMetaDatas,
                CreatedOn = message.CreatedOn,
                CreatedBy = message.CreatedBy
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
                CreatedBy = photo.CreatedBy
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

        public async Task<ChatHubRoom> GetOneVsOneRoom(int callerUserId, int targetUserId, int moduleId)
        {
            var callerUser = await this.chatHubRepository.GetUserByIdAsync(callerUserId);
            var targetUser = await this.chatHubRepository.GetUserByIdAsync(targetUserId);

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
