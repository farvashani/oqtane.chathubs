using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Oqtane.ChatHubs.Hubs;
using Oqtane.ChatHubs.Repository;
using Oqtane.ChatHubs.Services;

namespace Oqtane.ChatHubs.Commands
{
    public class CommandServicesContext
    {
        public ChatHub ChatHub { get; set; }
        public IChatHubRepository ChatHubRepository { get; set; }
        public IChatHubService ChatHubService { get; set; }
        public UserManager<IdentityUser> UserManager { get; set; }
    }
}