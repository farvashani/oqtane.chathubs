using System;

namespace BlazorAlerts
{
    public interface IBlazorAlertsService
    {

        void NewBlazorAlert(string message, string heading, PositionType position = PositionType.Fixed);

        void RemoveAlert(Guid guid);

    }
}
