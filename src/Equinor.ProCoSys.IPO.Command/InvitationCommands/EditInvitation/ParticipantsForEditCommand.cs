using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class ParticipantsForEditCommand : ParticipantsForCommand
    {
        public ParticipantsForEditCommand(
            Organization organization,
            InvitedExternalEmailForEditCommand invitedExternalEmail,
            InvitedPersonForEditCommand invitedPerson,
            InvitedFunctionalRoleForEditCommand invitedFunctionalRole,
            int sortKey) : base(organization, invitedExternalEmail, invitedPerson, invitedFunctionalRole, sortKey)
        {
            InvitedExternalEmailToEdit = invitedExternalEmail;
            InvitedPersonToEdit = invitedPerson;
            InvitedFunctionalRoleToEdit = invitedFunctionalRole;
        }

        public InvitedExternalEmailForEditCommand InvitedExternalEmailToEdit { get; }
        public InvitedPersonForEditCommand InvitedPersonToEdit { get; }
        public InvitedFunctionalRoleForEditCommand InvitedFunctionalRoleToEdit { get; }
    }
}
