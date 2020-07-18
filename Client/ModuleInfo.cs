using Oqtane.Models;
using Oqtane.Modules;

namespace Oqtane.StreamHubs
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "StreamHubs",
            Description = "StreamHubs",
            Version = "1.0.0",
            ServerManagerType = "Oqtane.StreamHubs.Manager.StreamHubManager, Oqtane.StreamHubs.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Oqtane.StreamHubs.Shared.Oqtane"
        };
    }
}
