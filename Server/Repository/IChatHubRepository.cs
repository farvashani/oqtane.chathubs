using System;
using System.Linq;
using System.Threading.Tasks;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public interface IChatHubRepository
    {

        #region GET

        IQueryable<ChatHubRoom> GetChatHubRooms(int ModuleId);
        IQueryable<ChatHubRoom> GetChatHubRoomsByUser(ChatHubUser user);
        IQueryable<ChatHubUser> GetChatHubUsersByRoom(ChatHubRoom room);
        ChatHubRoom GetChatHubRoom(int ChatHubRoomId);
        ChatHubRoom GetChatHubRoomOneVsOne(string OneVsOneId);
        IQueryable<ChatHubMessage> GetChatHubMessages(int roomId, TimeSpan timespan);
        ChatHubMessage GetChatHubMessage(int ChatHubMessageId);
        IQueryable<ChatHubUser> GetOnlineUsers();
        IQueryable<ChatHubUser> GetOnlineUsers(int roomId);
        IQueryable<ChatHubConnection> GetConnectionsByUserId(int userId);
        Task<ChatHubConnection> GetConnectionByConnectionId(string connectionId);
        ChatHubRoomChatHubUser GetChatHubRoomChatHubUser(int chatHubRoomId, int chatHubUserId);
        IQueryable<ChatHubIgnore> GetIgnoredUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredByApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubIgnore> GetIgnoredByUsers(ChatHubUser user);
        ChatHubSetting GetChatHubSetting(int ChatHubSettingId);
        ChatHubSetting GetChatHubSettingByUser(ChatHubUser user);
        Task<ChatHubUser> GetUserByIdAsync(int id);
        Task<ChatHubUser> GetUserByUserNameAsync(string username);
        Task<ChatHubUser> GetUserByDisplayName(string displayName);

        #endregion

        #region ADD

        ChatHubRoom AddChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubMessage AddChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubConnection AddChatHubConnection(ChatHubConnection ChatHubConnection);
        ChatHubUser AddChatHubUser(ChatHubUser ChatHubUser);
        ChatHubRoomChatHubUser AddChatHubRoomChatHubUser(ChatHubRoomChatHubUser ChatHubRoomChatHubUser);
        ChatHubPhoto AddChatHubPhoto(ChatHubPhoto ChatHubPhoto);
        ChatHubIgnore AddChatHubIgnore(ChatHubIgnore chatHubIgnore);
        ChatHubSetting AddChatHubSetting(ChatHubSetting ChatHubSetting);

        #endregion

        #region DELETE

        void DeleteChatHubRoom(int ChatHubRoomId, int ModuleId);
        void DeleteChatHubMessage(int ChatHubMessageId, int ChatHubRoomId);
        void DeleteChatHubConnection(int ChatHubConnectionId, int ChatHubUserId);
        void DeleteChatHubRoomChatHubUser(int ChatHubRoomId, int ChatHubUserId);
        void DeleteChatHubIgnore(ChatHubIgnore chatHubIgnore);

        #endregion

        #region UPDATE

        ChatHubRoom UpdateChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubMessage UpdateChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubConnection UpdateChatHubConnection(ChatHubConnection ChatHubConnection);
        ChatHubIgnore UpdateChatHubIgnore(ChatHubIgnore chatHubIgnore);
        ChatHubSetting UpdateChatHubSetting(ChatHubSetting ChatHubSetting);

        #endregion

    }
}
