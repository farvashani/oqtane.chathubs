using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Repository
{
    public static class ChatHubRepositoryExtensionMethods
    {

        public static IQueryable<ChatHubRoom> Active(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(r => r.Status == (int)ChatHubRoomStatus.Active);
        }

        public static IQueryable<ChatHubConnection> Active(this IQueryable<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == (int)ChatHubConnectionStatus.Active);
        }

        public static IEnumerable<ChatHubConnection> Active(this ICollection<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == (int)ChatHubConnectionStatus.Active);
        }

    }
}
