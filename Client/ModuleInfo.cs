using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.ChatHubs
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "ChatHub",
            Description = "ChatHub",
            Version = "1.0.0",
            ServerManagerType = "Oqtane.ChatHubs.Manager.ChatHubManager, Oqtane.ChatHubs.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Oqtane.ChatHubs.Shared.Oqtane"
        };
    }
}
