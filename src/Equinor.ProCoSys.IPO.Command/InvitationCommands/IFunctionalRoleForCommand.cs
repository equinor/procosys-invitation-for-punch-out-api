using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public interface IFunctionalRoleForCommand
    {
        public string Code { get; }
        public IEnumerable<IPersonForCommand> Persons { get; }
    }
}
