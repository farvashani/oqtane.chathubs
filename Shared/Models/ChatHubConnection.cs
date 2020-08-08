using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubConnection : ChatHubBaseModel
    {

        public int ChatHubUserId { get; set; }
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }

        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}