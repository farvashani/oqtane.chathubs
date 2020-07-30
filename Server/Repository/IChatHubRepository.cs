using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public interface IChatHubRepository
    {

        IEnumerable<ChatHubRoom> GetChatHubRooms(int ModuleId);
        IQueryable<ChatHubRoom> GetChatHubRoomsByUser(ChatHubUser user);
        ChatHubRoom AddChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubRoom UpdateChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubRoom GetChatHubRoom(int ChatHubRoomId);
        void DeleteChatHubRoom(int ChatHubRoomId, int ModuleId);

        IEnumerable<ChatHubMessage> GetChatHubMessages(int ChatHubRoomId);
        ChatHubMessage AddChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubMessage UpdateChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubMessage GetChatHubMessage(int ChatHubMessageId);
        void DeleteChatHubMessage(int ChatHubMessageId, int ChatHubRoomId);

        ChatHubConnection AddChatHubConnection(ChatHubConnection ChatHubConnection);
        void DeleteChatHubConnection(int ChatHubConnectionId, int ChatHubUserId);

        IQueryable<ChatHubUser> GetOnlineUsers();
        IQueryable<ChatHubUser> GetOnlineUsers(ChatHubRoom room);

        ChatHubConnection UpdateChatHubConnection(ChatHubConnection ChatHubConnection);

        IQueryable<ChatHubConnection> GetConnectionsByUserId(int userId);
        Task<ChatHubConnection> GetConnectionByConnectionId(string connectionId);

        ChatHubUser AddChatHubUser(ChatHubUser ChatHubUser);

        ChatHubRoomChatHubUser GetChatHubRoomChatHubUser(int chatHubRoomId, int chatHubUserId);
        ChatHubRoomChatHubUser AddChatHubRoomChatHubUser(ChatHubRoomChatHubUser ChatHubRoomChatHubUser);
        void DeleteChatHubRoomChatHubUser(int ChatHubRoomId, int ChatHubUserId);

        ChatHubPhoto AddChatHubPhoto(ChatHubPhoto ChatHubPhoto);

        IQueryable<ChatHubIgnore> GetIgnoredUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredByApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubIgnore> GetIgnoredByUsers(ChatHubUser user);
        ChatHubIgnore AddChatHubIgnore(ChatHubIgnore chatHubIgnore);
        ChatHubIgnore UpdateChatHubIgnore(ChatHubIgnore chatHubIgnore);
        void DeleteChatHubIgnore(ChatHubIgnore chatHubIgnore);

        Task<ChatHubUser> GetUserByIdAsync(int id);
        Task<ChatHubUser> GetUserByUserNameAsync(string username);
        IQueryable<ChatHubUser> GetUsersByDisplayName(string displayName);

    }
}
