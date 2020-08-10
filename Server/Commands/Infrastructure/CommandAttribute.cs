using Oqtane.ChatHubs.Server.Resources;
using System;
using System.Globalization;
using System.Resources;

namespace Oqtane.ChatHubs.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string ResourceName { get; set; }
        public string[] Commands { get; set; }
        public string Arguments { get; set; }
        public string Usage { get; set; }
        public string[] Roles { get; set; }

        public CommandAttribute(string resourceName, string arguments, string[] roles, string usage)
        {
            this.ResourceName = resourceName;

            string commandResourceString = new ResourceManager(typeof(CommandResources)).GetString(resourceName, CultureInfo.CurrentCulture);
            string[] commands = commandResourceString.Split(';');
            this.Commands = commands;

            this.Arguments = arguments;
            this.Roles = roles;
            this.Usage = usage;
        }
    }
}
