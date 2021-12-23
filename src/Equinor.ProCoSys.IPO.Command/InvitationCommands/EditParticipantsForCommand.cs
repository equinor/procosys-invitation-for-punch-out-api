using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class EditParticipantsForCommand
    {
        public EditParticipantsForCommand(
            Organization organization,
            EditExternalEmailForCommand externalEmail,
            EditPersonForCommand person,
            EditFunctionalRoleForCommand functionalRole,
            int sortKey)
        {
            Organization = organization;
            FunctionalRole = functionalRole;
            Person = person;
            ExternalEmail = externalEmail;
            SortKey = sortKey;
        }
        public Organization Organization { get; }
        public int SortKey { get; }
        public EditExternalEmailForCommand ExternalEmail { get; }
        public EditPersonForCommand Person { get; }
        public EditFunctionalRoleForCommand FunctionalRole { get; }
    }
}
