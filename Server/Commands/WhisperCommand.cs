using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Linq;
using Oqtane.ChatHubs.Repository;
using System;
using Oqtane.Shared.Enums;
using System.Collections.Generic;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("whisper", "[username]", new string[] { Constants.AllUsersRole, Constants.AdminRole }, "Usage: /whisper | /asmr")]
    public class WhisperCommand : BaseCommand
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

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = callerContext.RoomId,
                ChatHubUserId = caller.UserId,
                User = caller,
                Content = msg ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Whisper)
            };
            context.ChatHubRepository.AddChatHubMessage(chatHubMessage);
            var chatHubMessageClientModel = context.ChatHubService.CreateChatHubMessageClientModel(chatHubMessage);

            var users = new List<ChatHubUser>(); users.Add(caller); users.Add(targetUser);

            foreach (var item in users)
            {
                var rooms = context.ChatHubRepository.GetChatHubRoomsByUser(item).Public().Active().ToList();
                var connections = item.Connections.Active();

                foreach (var room in rooms)
                {
                    foreach (var connection in connections)
                    {
                        await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("AddMessage", chatHubMessageClientModel);
                    }
                }
            }

        }
    }
}