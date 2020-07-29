using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.ChatHubs.Commands
{
    public class CommandCallerContext
    {
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
        public int RoomId { get; set; }
    }
}