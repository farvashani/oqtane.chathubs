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

            /*
            IList<ChatHubMessage> recentMessages = chatRoom.ChatRoomType == (int)ChatRoomTypeEnum.Separee
                                                ? await _repository.GetMessagesByRoom(chatRoom, 10).OrderBy(m => m.Created).ToListAsync()
                                                : new List<ChatMessage>();
                                                */
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
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn
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

        public async Task<ChatHubUser> IdentifyGuest(string connectionId)
        {
            ChatHubConnection connection = await Task.Run(() => chatHubRepository.GetConnectionByConnectionId(connectionId));
            if (connection != null)
            {
                return connection.User;
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

    }
}
