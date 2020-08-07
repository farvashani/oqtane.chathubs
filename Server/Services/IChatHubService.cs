using Oqtane.Modules;
using Oqtane.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public interface IChatHubService
    {

        Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room);

        ChatHubUser CreateChatHubUserClientModel(ChatHubUser chatHubUser);

        ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message);

        ChatHubPhoto CreateChatHubPhotoClientModel(ChatHubPhoto photo);

        void IgnoreUser(ChatHubUser guest, ChatHubUser targetUser);

        Task<ChatHubUser> IdentifyGuest(string connectionId);

        List<string> GetAllExceptConnectionIds(ChatHubUser user);

        Task<ChatHubRoom> GetOneVsOneRoom(int callerUserId, int targetUserId, int moduleId);

        string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2);

        bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller);

    }
}
