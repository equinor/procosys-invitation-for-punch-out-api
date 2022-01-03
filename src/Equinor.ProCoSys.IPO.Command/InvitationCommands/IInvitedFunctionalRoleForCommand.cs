using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public interface IInvitedFunctionalRoleForCommand
    {
        public string Code { get; }
        public IEnumerable<IInvitedPersonForCommand> InvitedPersons { get; }
    }
}
