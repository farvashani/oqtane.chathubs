using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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

        Task<ChatHubUser> IdentifyUser(HubCallerContext Context);

        List<string> GetAllExceptConnectionIds(ChatHubUser user);

        ChatHubRoom GetOneVsOneRoom(ChatHubUser caller, ChatHubUser targetUser, int moduleId);

        string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2);

        bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller);

    }
}
