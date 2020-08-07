using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoom : IAuditable
    {

        public int ChatHubRoomId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string OneVsOneId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubUser> RoomUsers { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubMessage> Messages { get; set; }

        [NotMapped]
        public string MessageInput { get; set; }
        [NotMapped]
        public int UnreadMessages { get; set; } = 0;
        [NotMapped]
        public bool ShowUserlist { get; set; }
        [NotMapped]
        public virtual IList<ChatHubUser> Users { get; set; }

    }
}
