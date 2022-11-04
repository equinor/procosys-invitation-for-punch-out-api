﻿using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class IpoSendMailException : Exception
    {
        public IpoSendMailException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
