using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.Shared.Enums;
using System;
using System.Linq;
using Oqtane.ChatHubs.Repository;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("invite", "[username]", new string[] { Constants.AllUsersRole, Constants.AdminRole } , "Usage: /invite")]
    public class InviteCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByDisplayName(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (!targetUser.Online())
            {
                await context.ChatHub.SendClientNotification("User not online.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (caller.UserId == targetUser.UserId)
            {
                await context.ChatHub.SendClientNotification("Calling user can not be target user.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string msg = null;
            if (args.Length > 1)
            {
                msg = String.Join(" ", args.Skip(1)).Trim();
            }

            var callerRoom = context.ChatHubRepository.GetChatHubRoom(callerContext.RoomId);
            var oneVsOneRoom = await context.ChatHubService.GetOneVsOneRoom(callerContext.UserId, targetUser.UserId, callerRoom.ModuleId);

            if(oneVsOneRoom != null)
            {
                await context.ChatHub.EnterChatRoom(oneVsOneRoom.ChatHubRoomId);

                ChatHubInvitation chatHubInvitation = new ChatHubInvitation()
                {
                    Guid = Guid.NewGuid(),
                    RoomId = oneVsOneRoom.ChatHubRoomId,
                    Hostname = caller.DisplayName
                };
                foreach(var connection in targetUser.Connections.Active())
                {
                    await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("AddInvitation", chatHubInvitation);
                }
            }

        }
    }
}