using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubIgnore : ChatHubBaseModel
    {

        public int ChatHubUserId { get; set; }
        public int ChatHubIgnoredUserId { get; set; }


        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}