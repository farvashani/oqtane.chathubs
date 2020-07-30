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
            return rooms.Where(r => r.Status == Enum.GetName(typeof(ChatHubRoomStatus), ChatHubRoomStatus.Active));
        }

        public static IQueryable<ChatHubConnection> Active(this IQueryable<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active));
        }

        public static IEnumerable<ChatHubConnection> Active(this ICollection<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active));
        }

        public static bool Online(this ChatHubUser user)
        {
            return user.Connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)).Any();
        }

        public static IQueryable<ChatHubRoom> Public(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(room => room.Type == Enum.GetName(typeof(ChatHubRoomType), ChatHubRoomType.Public));
        }

    }
}
