using Microsoft.AspNetCore.Components;
using Oqtane.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Oqtane.ChatHubs.BlazorAlerts
{
    public class BlazorAlertsService : ServiceBase, IBlazorAlertsService
    {

        private readonly HttpClient _httpClient;
        public event Action<string, string> OnAlert;

        public List<BlazorAlertsModel> BlazorAlerts { get; set; } = new List<BlazorAlertsModel>();

        public BlazorAlertsService(HttpClient httpClient) : base(httpClient)
        {
            this._httpClient = httpClient;
        }

        public void NewBlazorAlert(string message, string heading)
        {
            OnAlert?.Invoke(message, heading);
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
