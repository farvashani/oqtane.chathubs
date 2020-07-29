using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Commands
{
    public abstract class AdminCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext commandServicesContext, CommandCallerContext commandCallerContext, string[] args, ChatHubUser caller)
        {
            IdentityUser identityUser = await commandServicesContext.UserManager.FindByNameAsync(caller.Username);
            if (!commandServicesContext.UserManager.IsInRoleAsync(identityUser, Oqtane.Shared.Constants.AdminRole).Result)
            {
                return;
            }

            await ExecuteAdminOperation(commandServicesContext, commandCallerContext, args, caller);
        }

        public abstract Task ExecuteAdminOperation(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller);
    }
}
