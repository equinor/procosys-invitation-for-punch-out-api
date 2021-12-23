using System.Collections.Generic;
using System.Linq;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class EditFunctionalRoleForCommand : IFunctionalRoleForCommand
    {
        public EditFunctionalRoleForCommand(
            string code,
            IEnumerable<EditPersonForCommand> persons,
            int? id = null,
            string rowVersion = null)
        {
            Code = code;
            EditPersons = persons ?? new List<EditPersonForCommand>();
            Id = id;
            RowVersion = rowVersion;
        }
        public string Code { get; }
        public IEnumerable<IPersonForCommand> Persons => EditPersons.Cast<IPersonForCommand>();
        public IEnumerable<EditPersonForCommand> EditPersons { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
