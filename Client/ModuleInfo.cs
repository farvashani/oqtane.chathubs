using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.StreamHubs
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "ChatHub",
            Description = "ChatHub",
            Version = "1.0.0",
            ServerManagerType = "Oqtane.StreamHubs.Manager.ChatHubManager, Oqtane.StreamHubs.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Oqtane.StreamHubs.Shared.Oqtane"
        };
    }
}
