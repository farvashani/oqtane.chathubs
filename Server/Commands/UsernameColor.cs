using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("username-color", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /username-color")]
    public class UsernameColor : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            string usernameColor = args[0];

            if(!string.IsNullOrEmpty(usernameColor))
            {
                var settings = caller.Settings;
                settings.UsernameColor = usernameColor;
                context.ChatHubRepository.UpdateChatHubSetting(settings);

                await context.ChatHub.SendNotification("Username Color Updated.", callerContext.RoomId, callerContext.ConnectionId, caller);
            }

        }
    }
}