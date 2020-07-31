using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using System;
using Oqtane.Shared.Enums;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("commands", "[]", new string[] { Constants.AllUsersRole, Constants.AdminRole }, "Usage: /commands | /list-commands")]
    public class ListCommandsCommand : BaseCommand
    {

        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            await context.ChatHub.SendCommandMetaDatas(callerContext.RoomId);            

        }

    }
}