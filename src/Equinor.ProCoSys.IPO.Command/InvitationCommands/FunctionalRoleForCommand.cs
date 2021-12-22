using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class FunctionalRoleForCommand
    {
        public FunctionalRoleForCommand(
            string code,
            IList<PersonForCommand> persons,
            int? id = null)
        {
            Code = code;
            Persons = persons ?? new List<PersonForCommand>();
            Id = id;
        }
        public string Code { get; }
        public IList<PersonForCommand> Persons { get; }
        public int? Id { get; }
    }
}
