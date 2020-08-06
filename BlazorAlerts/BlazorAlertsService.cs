using System;
using System.Collections.Generic;
using System.Linq;

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

        public void AddAlert(BlazorAlertsModel model)
        {
            if(!this.BlazorAlerts.Any(item => item.Guid == model.Guid))
            {
                this.BlazorAlerts.Add(model);
            }
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
