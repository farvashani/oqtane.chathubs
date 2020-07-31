namespace Oqtane.Shared.Models
{
    public class ChatHubCommandMetaData
    {
        public string ResourceName { get; set; }
        public string[] Commands { get; set; }        
        public string Arguments { get; set; }
        public string[] Roles { get; set; }
        public string Usage { get; set; }
    }
}