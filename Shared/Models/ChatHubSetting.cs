using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.Shared.Models
{
    public class ChatHubSetting : IAuditable
    {

        public int ChatHubSettingId { get; set; }
        public string UsernameColor { get; set; }
        public string MessageColor { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        public int ChatHubUserId { get; set; }
        public ChatHubUser User { get; set; }

    }
}
