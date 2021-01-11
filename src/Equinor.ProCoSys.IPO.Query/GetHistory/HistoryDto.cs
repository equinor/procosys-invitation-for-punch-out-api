using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetHistory
{
    public class HistoryDto
    {
        public HistoryDto(
            int id,
            string description,
            DateTime createdAtUtc,
            PersonDto createdBy,
            EventType eventType)
        {
            Id = id;
            Description = description;
            CreatedBy = createdBy;
            CreatedAtUtc = createdAtUtc;
            EventType = eventType;
        }

        public int Id { get; }
        public string Description { get; }
        public DateTime CreatedAtUtc { get; }
        public PersonDto CreatedBy { get; }
        public EventType EventType { get; }
    }
}
