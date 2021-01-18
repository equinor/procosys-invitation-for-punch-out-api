using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class IpoValidationException : Exception
    {
        public IpoValidationException(string error) : base(error)
        {
        }
    }
}
