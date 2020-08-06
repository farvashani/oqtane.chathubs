using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;

namespace BlazorAlerts
{
    public class BlazorAlertsBase : ComponentBase
    {

        [Inject]
        public BlazorAlertsService BlazorAlertsService { get; set; }

        public BlazorAlertsBase() 
        {
            
        }

        protected override void OnInitialized()
        {
            this.BlazorAlertsService.OnAlert += OnAlertExecute;
        }

        public async void OnAlertExecute(string message, string heading, PositionType position)
        {
            await InvokeAsync(() =>
            {
                BlazorAlertsModel alert = new BlazorAlertsModel()
                {
                    Guid = Guid.NewGuid(),
                    Message = message,
                    Headline = heading,
                    Position = position,
                    CreatedOn = DateTime.Now
                };

                this.BlazorAlertsService.AddAlert(alert);
                StateHasChanged();
            });
        }

        public void CloseAlert_OnClicked(Guid guid)
        {
            this.BlazorAlertsService.RemoveAlert(guid);
        }

    }
}
