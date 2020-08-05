using System;

namespace BlazorAlerts
{
    public class BlazorAlertsModel
    {

        public Guid Guid { get; set; }

        public string Message { get; set; }

        public string Headline { get; set; }

        public PositionType Position { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
