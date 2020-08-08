using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoom : ChatHubBaseModel
    {

        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string OneVsOneId { get; set; }


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
