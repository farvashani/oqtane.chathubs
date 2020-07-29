namespace Oqtane.ChatHubs.Commands
{
    public class CommandMetaData
    {
        public string ResourceName { get; set; }
        public string[] Commands { get; set; }        
        public string Arguments { get; set; }
        public string Usage { get; set; }
        public string Roles { get; set; }
    }
}