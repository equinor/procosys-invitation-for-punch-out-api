using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ParticipantsForCommand
    {
        public ParticipantsForCommand(
            Organization organization,
            IInvitedExternalEmailForCommand invitedExternalEmail,
            IInvitedPersonForCommand invitedPerson,
            IInvitedFunctionalRoleForCommand invitedFunctionalRole,
            int sortKey)
        {
            Organization = organization;
            InvitedFunctionalRole = invitedFunctionalRole;
            InvitedPerson = invitedPerson;
            InvitedExternalEmail = invitedExternalEmail;
            SortKey = sortKey;
        }
        public Organization Organization { get; }
        public int SortKey { get; }
        public IInvitedExternalEmailForCommand InvitedExternalEmail { get; }
        public IInvitedPersonForCommand InvitedPerson { get; }
        public IInvitedFunctionalRoleForCommand InvitedFunctionalRole { get; }
    }
}
