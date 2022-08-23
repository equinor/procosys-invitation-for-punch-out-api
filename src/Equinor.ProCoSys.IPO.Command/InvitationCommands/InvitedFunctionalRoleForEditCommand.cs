using System.Collections.Generic;
using System.Linq;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class InvitedFunctionalRoleForEditCommand : IInvitedFunctionalRoleForCommand
    {
        public InvitedFunctionalRoleForEditCommand(
            int? id,
            string code,
            IEnumerable<InvitedPersonForEditCommand> persons,
            string rowVersion)
        {
            Id = id;
            Code = code;
            EditPersons = persons ?? new List<InvitedPersonForEditCommand>();
            RowVersion = rowVersion;
        }

        // null for Id will add new
        public int? Id { get; }
        public string Code { get; }
        public IEnumerable<IInvitedPersonForCommand> InvitedPersons => EditPersons.Cast<IInvitedPersonForCommand>();
        public IEnumerable<InvitedPersonForEditCommand> EditPersons { get; }
        public string RowVersion { get; }
    }
}
