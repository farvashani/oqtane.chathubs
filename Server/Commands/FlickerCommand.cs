using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Text.RegularExpressions;
using System;
using Oqtane.Shared.Enums;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("flicker", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /flicker")]
    public class FlickerCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {
            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string msg = String.Join(" ", args).Trim();

            var regex = new Regex(@"[^ bcdfghjklmnpqrstvwxyz_]");
            msg = regex.Replace(msg, "");

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = callerContext.RoomId,
                ChatHubUserId = caller.UserId,
                User = caller,
                Content = msg,
                Type = ChatHubMessageType.Guest.ToString()
            };
            context.ChatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = context.ChatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            var connectionsIds = context.ChatHubService.GetAllExceptConnectionIds(caller);
            await context.ChatHub.Clients.GroupExcept(callerContext.RoomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);

        }
    }
}