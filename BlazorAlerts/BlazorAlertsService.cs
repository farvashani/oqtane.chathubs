using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BlazorAlerts
{
    public class BlazorAlertsService : IBlazorAlertsService
    {

        public event Action<string, string, PositionType> OnAlert;

        public List<BlazorAlertsModel> BlazorAlerts { get; set; } = new List<BlazorAlertsModel>();

        public BlazorAlertsService()
        {

        }

        public void NewBlazorAlert(string message, string heading, PositionType position = PositionType.Fixed)
        {
            this.OnAlert?.Invoke(message, heading, position);
        }

        public void RemoveAlert(Guid guid)
        {
            BlazorAlertsModel item = this.BlazorAlerts.FirstOrDefault(item => item.Guid == guid);
            if(item != null)
            {
                this.BlazorAlerts.Remove(item);
            }
        }

    }
}
