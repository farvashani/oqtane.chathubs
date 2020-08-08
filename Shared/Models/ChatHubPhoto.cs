using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubPhoto : ChatHubBaseModel
    {

        public int ChatHubMessageId { get; set; }
        public string Source { get; set; }
        public string Thumb { get; set; }
        public string Caption { get; set; }
        public long Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        
        [NotMapped]
        public virtual ChatHubMessage Message { get; set; }

    }
}
