using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Linq;
using Oqtane.ChatHubs.Repository;
using System;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("ignore", "[username]", Constants.AllUsersRole , "Usage: /ignore | /block")]
    public class IgnoreCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByDisplayName(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            await context.ChatHub.IgnoreUser(targetUser.Username);

        }
    }
}