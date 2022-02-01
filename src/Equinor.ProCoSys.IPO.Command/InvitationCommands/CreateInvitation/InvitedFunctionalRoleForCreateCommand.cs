using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class InvitedFunctionalRoleForCreateCommand : IInvitedFunctionalRoleForCommand
    {
        public InvitedFunctionalRoleForCreateCommand(
            string code,
            IEnumerable<IInvitedPersonForCommand> invitedPersons)
        {
            Code = code;
            InvitedPersons = invitedPersons ?? new List<IInvitedPersonForCommand>();
        }

        public string Code { get; }
        public IEnumerable<IInvitedPersonForCommand> InvitedPersons { get; }
    }
}
