using Microsoft.AspNetCore.Components;
using Oqtane.Modules;
using System;
using System.Net.Http;

namespace Oqtane.ChatHubs.BlazorAlerts
{
    public class BlazorAlertsBase : ModuleBase
    {

        [Inject]
        protected HttpClient HttpClient { get; set; }

        [Parameter]
        public BlazorAlertsService BlazorAlertsService { get; set; }

        public BlazorAlertsBase() 
        {
            
        }

        protected override void OnInitialized()
        {
            this.BlazorAlertsService.OnAlert += OnAlertExecute;
        }

        public async void OnAlertExecute(string message, string heading)
        {
            await InvokeAsync(() =>
            {
                BlazorAlertsModel alert = new BlazorAlertsModel()
                {
                    Guid = Guid.NewGuid(),
                    Message = message,
                    Headline = heading,
                    CreatedOn = DateTime.Now
                };

                this.BlazorAlertsService.BlazorAlerts.Add(alert);
                StateHasChanged();
            });
        }

        public void CloseAlert_OnClicked(Guid guid)
        {
            this.BlazorAlertsService.RemoveAlert(guid);
        }

    }
}
