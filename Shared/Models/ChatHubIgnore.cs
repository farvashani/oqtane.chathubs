using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.Shared.Models
{
    public class ChatHubIgnore : IAuditable
    {

        public int ChatHubIgnoreId { get; set; }
        public int ChatHubUserId { get; set; }
        public int ChatHubIgnoredUserId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}