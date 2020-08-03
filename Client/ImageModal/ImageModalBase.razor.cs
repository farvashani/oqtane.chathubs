using Microsoft.AspNetCore.Components;
using Oqtane.Modules;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs
{
    public class ImageModalBase : ModuleBase
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        public bool DialogIsOpen { get; set; }

        public ChatHubMessage Message { get; set; }

        public ImageModalBase() { }

        public void OpenDialog(ChatHubMessage item)
        {
            this.Message = item;
            this.DialogIsOpen = true;
            StateHasChanged();
        }

        public void CloseDialogClicked()
        {
            this.DialogIsOpen = false;
        }

    }
}
