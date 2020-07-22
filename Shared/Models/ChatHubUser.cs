using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Oqtane.Shared.Models
{

    public class ChatHubUser : User
    {

        [NotMapped]
        public bool UserlistItemCollapsed { get; set; } = false;

        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubUser> UserRooms { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubConnection> Connections { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubIgnore> Ignores { get; set; }

    }
}