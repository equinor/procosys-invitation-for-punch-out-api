using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Query;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class HistoryDto
    {
        public int Id { get; }
        public string Description { get; }
        public DateTime CreatedAtUtc { get; }
        public PersonMinimalDto CreatedBy { get; }
        public EventType EventType { get; }
    }
}
