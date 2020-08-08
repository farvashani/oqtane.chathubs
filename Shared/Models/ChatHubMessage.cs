using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubMessage : ChatHubBaseModel
    {

        public int ChatHubRoomId { get; set; }
        public int ChatHubUserId { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }


        [NotMapped]
        public virtual ChatHubRoom Room { get; set; }
        [NotMapped]
        public virtual ChatHubUser User { get; set; }
        [NotMapped]
        public virtual IList<ChatHubPhoto> Photos { get; set; }
        [NotMapped]
        public virtual IList<ChatHubCommandMetaData> CommandMetaDatas { get; set; }

    }
}