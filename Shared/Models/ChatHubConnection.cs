using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Shared.Models
{
    public class ChatHubConnection : IAuditable
    {

        public int ChatHubConnectionId { get; set; }

        public int ChatHubUserId { get; set; }
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public int Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        //[JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}