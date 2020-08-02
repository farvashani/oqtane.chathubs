using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.ChatHubs.BlazorAlerts
{
    public class BlazorAlertsModel
    {

        public Guid Guid { get; set; }

        public string Message { get; set; }

        public string Headline { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
