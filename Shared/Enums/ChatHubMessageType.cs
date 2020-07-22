using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.Shared.Enums
{
    public enum ChatHubMessageType
    {
        System = 0,
        Admin = 1,
        User = 2,
        Guest = 3,        
        JoinEnterLeaveExit = 4,
        Whisper = 5,
        Me = 6,
        Image = 7
    }
}
