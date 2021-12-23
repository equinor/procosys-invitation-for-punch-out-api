using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public interface IPersonForCommand
    {
        Guid? AzureOid { get; }
        string Email { get; }
        bool Required { get; }
    }
}
