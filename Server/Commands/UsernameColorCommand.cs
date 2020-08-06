using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.Shared.Enums;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("username-color", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /username-color")]
    public class UsernameColorCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string usernameColor = args[0];

            if(!string.IsNullOrEmpty(usernameColor))
            {
                var settings = caller.Settings;
                settings.UsernameColor = usernameColor;
                context.ChatHubRepository.UpdateChatHubSetting(settings);

                await context.ChatHub.SendClientNotification("Username Color Updated.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
            }

        }
    }
}