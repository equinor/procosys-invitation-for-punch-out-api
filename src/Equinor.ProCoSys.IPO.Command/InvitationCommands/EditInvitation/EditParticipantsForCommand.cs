using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditParticipantsForCommand : ParticipantsForCommand
    {
        public EditParticipantsForCommand(
            Organization organization,
            IExternalEmailForCommand externalEmail,
            IPersonForCommand person,
            IFunctionalRoleForCommand functionalRole,
            int sortKey) : base(organization, externalEmail, person, functionalRole, sortKey)
        {
        }

        public EditExternalEmailForCommand EditExternalEmail => ExternalEmail as EditExternalEmailForCommand;
        public EditPersonForCommand EditPerson => Person as EditPersonForCommand;
        public EditFunctionalRoleForCommand EditFunctionalRole => FunctionalRole as EditFunctionalRoleForCommand;
    }
}
