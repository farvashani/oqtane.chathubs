using Oqtane.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubSetting : ChatHubBaseModel
    {

        public string UsernameColor { get; set; }
        public string MessageColor { get; set; }


        public int ChatHubUserId { get; set; }
        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}
