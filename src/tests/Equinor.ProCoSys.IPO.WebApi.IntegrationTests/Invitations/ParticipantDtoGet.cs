using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class ParticipantDtoGet
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public ExternalEmailDto ExternalEmail { get; set; }
        public InvitedPersonDto Person { get; set; }
        public FunctionalRoleDto FunctionalRole { get; set; }
    }
}
