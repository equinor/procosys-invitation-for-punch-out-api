using System.Collections.Generic;
using System.Linq;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditFunctionalRoleForCommand : IFunctionalRoleForCommand
    {
        public EditFunctionalRoleForCommand(
            int? id,
            string code,
            IEnumerable<EditPersonForCommand> persons,
            string rowVersion)
        {
            Id = id;
            Code = code;
            EditPersons = persons ?? new List<EditPersonForCommand>();
            RowVersion = rowVersion;
        }

        // null for Id will add new
        public int? Id { get; }
        public string Code { get; }
        public IEnumerable<IPersonForCommand> Persons => EditPersons.Cast<IPersonForCommand>();
        public IEnumerable<EditPersonForCommand> EditPersons { get; }
        public string RowVersion { get; }
    }
}
