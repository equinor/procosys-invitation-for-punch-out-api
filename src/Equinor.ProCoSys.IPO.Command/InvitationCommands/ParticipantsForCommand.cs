using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ParticipantsForCommand
    {
        public ParticipantsForCommand(
            Organization organization,
            ExternalEmailForCommand externalEmail,
            PersonForCommand person,
            FunctionalRoleForCommand functionalRole,
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
        public ExternalEmailForCommand ExternalEmail { get; }
        public PersonForCommand Person { get; }
        public FunctionalRoleForCommand FunctionalRole { get; }
    }
}
