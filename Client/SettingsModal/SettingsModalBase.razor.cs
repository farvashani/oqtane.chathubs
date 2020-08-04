using BlazorStrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.ChatHubs
{
    public class SettingsModalBase : ModuleBase
    {

        [Parameter]
        public ChatHubService ChatHubService { get; set; }

        public BSModal CenteredBSModal;

        public void Toggle()
        {
            this.CenteredBSModal.Toggle();
        }

        public void BSModalOnToggle(MouseEventArgs e)
        {
            this.Toggle();
        }

    }
}
