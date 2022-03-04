using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class OutstandingIpoDetailsDto
    {
        public int InvitationId { get; set; }
        public string Description { get; set; }
        public Organization Organization { get; set; }
    }
}
