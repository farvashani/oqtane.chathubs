using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.ChatHubs.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string[] Names { get; set; }
        public string Arguments { get; set; }
        public string Usage { get; set; }
        public string Roles { get; set; }

        public CommandAttribute(string[] names, string arguments, string roles, string usage)
        {
            this.Names = names;
            this.Arguments = arguments;
            this.Roles = roles;
            this.Usage = usage;
        }
    }
}
