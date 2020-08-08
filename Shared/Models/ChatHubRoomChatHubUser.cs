using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoomChatHubUser : ChatHubBaseModel
    {

        public int ChatHubRoomId { get; set; }
        public int ChatHubUserId { get; set; }


        [NotMapped]
        public virtual ChatHubUser User { get; set; }
        [NotMapped]
        public virtual ChatHubRoom Room { get; set; }

    }
}