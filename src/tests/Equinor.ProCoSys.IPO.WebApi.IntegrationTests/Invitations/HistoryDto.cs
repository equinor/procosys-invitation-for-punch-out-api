using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class HistoryDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public EventType EventType { get; set; }
    }
}
