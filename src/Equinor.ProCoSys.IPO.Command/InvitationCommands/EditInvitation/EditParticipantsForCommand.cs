using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditParticipantsForCommand : ParticipantsForCommand
    {
        public EditParticipantsForCommand(
            Organization organization,
            EditExternalEmailForCommand externalEmail,
            EditPersonForCommand person,
            EditFunctionalRoleForCommand functionalRole,
            int sortKey) : base(organization, externalEmail, person, functionalRole, sortKey)
        {
            EditExternalEmail = externalEmail;
            EditPerson = person;
            EditFunctionalRole = functionalRole;
        }

        public EditExternalEmailForCommand EditExternalEmail { get; }
        public EditPersonForCommand EditPerson { get; }
        public EditFunctionalRoleForCommand EditFunctionalRole { get; }
    }
}
