using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Services
{
    public interface IChatHubService
    {

        HubConnection Connection { get; set; }

        ChatHubUser ConnectedUser { get; set; }

        void BuildGuestConnection(string username);

        void RegisterHubConnectionHandlers();

        Task ConnectAsync();

        Task<List<ChatHubRoom>> GetChatHubRoomsAsync(int ModuleId);
        Task<ChatHubRoom> GetChatHubRoomAsync(int ChatHubRoomId, int ModuleId);
        Task<ChatHubRoom> AddChatHubRoomAsync(ChatHubRoom ChatHubRoom);
        Task UpdateChatHubRoomAsync(ChatHubRoom ChatHubRoom);
        Task DeleteChatHubRoomAsync(int ChatHubRoomId, int ModuleId);

        Task FixCorruptConnections(int ModuleId);

    }
}