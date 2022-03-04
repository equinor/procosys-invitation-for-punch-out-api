using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public interface IInvitedPersonForCommand
    {
        Guid? AzureOid { get; }
        string Email { get; }
        bool Required { get; }
    }
}
