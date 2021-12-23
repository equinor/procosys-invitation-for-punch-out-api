using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateFunctionalRoleForCommand : IFunctionalRoleForCommand
    {
        public CreateFunctionalRoleForCommand(
            string code,
            IEnumerable<IPersonForCommand> persons)
        {
            Code = code;
            Persons = persons ?? new List<IPersonForCommand>();
        }

        public string Code { get; }
        public IEnumerable<IPersonForCommand> Persons { get; }
    }
}
