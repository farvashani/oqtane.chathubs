using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubBaseModel : IAuditable, IChatHubBaseModel
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

    }
}
