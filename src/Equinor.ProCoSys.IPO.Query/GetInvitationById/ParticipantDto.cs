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
            FunctionalRoleDto functionalRole,
            string rowVersion)
        {
            Organization = organization;
            SortKey = sortKey;
            ExternalEmail = externalEmail;
            Person = person;
            FunctionalRole = functionalRole;
            RowVersion = rowVersion;
        }

        public Organization Organization { get; }
        public int SortKey { get; }
        public ExternalEmailDto ExternalEmail { get; }
        public PersonDto Person { get; }
        public FunctionalRoleDto FunctionalRole { get; }
        public string RowVersion { get; }
    }
}
