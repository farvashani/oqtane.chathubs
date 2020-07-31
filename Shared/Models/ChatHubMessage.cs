using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Shared.Models
{
    public class ChatHubMessage : IAuditable
    {

        public int ChatHubMessageId { get; set; }
        public int ChatHubRoomId { get; set; }
        public int ChatHubUserId { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public virtual ChatHubRoom Room { get; set; }
        [NotMapped]
        public virtual ChatHubUser User { get; set; }
        [NotMapped]
        public IList<ChatHubPhoto> Photos { get; set; }
        [NotMapped]
        public IList<ChatHubCommandMetaData> CommandMetaDatas { get; set; }

    }
}