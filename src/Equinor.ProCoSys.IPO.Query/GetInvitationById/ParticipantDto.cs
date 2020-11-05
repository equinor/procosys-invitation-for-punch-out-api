using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ParticipantDto
    {
        public ParticipantDto(
            Organization organization,
            int sortKey,
            ExternalEmailDto externalEmail,
            PersonDto person,
            FunctionalRoleDto functionalRole)
        {
            Organization = organization;
            SortKey = sortKey;
            ExternalEmail = externalEmail;
            Person = person;
            FunctionalRole = functionalRole;
        }

        public Organization Organization { get; }
        public int SortKey { get; }
        public ExternalEmailDto ExternalEmail { get; }
        public PersonDto Person { get; }
        public FunctionalRoleDto FunctionalRole { get; }
    }
}
