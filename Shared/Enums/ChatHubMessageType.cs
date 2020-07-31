using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Oqtane.Shared.Enums
{
    public enum ChatHubMessageType
    {
        System,
        Admin,
        User,
        Guest,
        Enter_Leave,
        Connect_Disconnect,
        Whisper,
        Me,
        Image,
        Commands
    }
}
