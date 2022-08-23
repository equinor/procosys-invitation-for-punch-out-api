using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public interface IInvitedPersonForCommand
    {
        Guid AzureOid { get; }
        bool Required { get; }
    }
}
