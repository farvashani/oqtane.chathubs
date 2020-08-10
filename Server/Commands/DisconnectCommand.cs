using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("disconnect", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /disconnect | /exit | /shutdown")]
    public class DisconnectCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            await context.ChatHub.Clients.Client(callerContext.ConnectionId).SendAsync("Disconnect", context.ChatHubService.CreateChatHubUserClientModel(caller));

        }
    }
}