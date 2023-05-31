using System;
using System.Transactions;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class IpoSendMailException : Exception
    {
        public IpoSendMailException()
        {
        }

        public IpoSendMailException(string? message, Exception? exception) : base(message, exception)
        {
        }
    }
}
