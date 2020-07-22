using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Shared.Models
{
    public class ChatHubPhoto : IAuditable
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChatHubPhotoId { get; set; }
        public int ChatHubMessageId { get; set; }
        public string Source { get; set; }
        public string Thumb { get; set; }
        public string Caption { get; set; }
        public long Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        
        [NotMapped]
        public virtual ChatHubMessage Message { get; set; }

    }
}
