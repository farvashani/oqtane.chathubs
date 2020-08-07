using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.Shared.Models
{
    public class ChatHubInvitation
    {

        public Guid Guid { get; set; }

        public int RoomId { get; set; }

        public string Hostname { get; set; }

    }
}
