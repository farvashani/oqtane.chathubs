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
    [Command("whisper", "[username]", Constants.AllUsersRole , "Usage: /whisper | /asmr")]
    public class WhisperCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = context.ChatHubRepository.GetUserByDisplayName(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            if (!targetUser.Online())
            {
                await context.ChatHub.SendNotification("User not online.", callerContext.RoomId, callerContext.ConnectionId, caller);
                return;
            }

            if (caller.UserId == targetUser.UserId)
            {
                await context.ChatHub.SendNotification("Calling user can not be target user.", callerContext.RoomId, callerContext.ConnectionId, caller);
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