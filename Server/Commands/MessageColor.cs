using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("message-color", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /message-color")]
    public class MessageColor : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            string messageColor = args[0];

            if(!string.IsNullOrEmpty(messageColor))
            {
                var settings = caller.Settings;
                settings.MessageColor = messageColor;
                context.ChatHubRepository.UpdateChatHubSetting(settings);

                await context.ChatHub.SendNotification("Message Color Updated.", callerContext.RoomId, callerContext.ConnectionId, caller);
            }

        }
    }
}