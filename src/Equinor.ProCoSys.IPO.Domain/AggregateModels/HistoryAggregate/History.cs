using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public class History : PlantEntityBase, IAggregateRoot, ICreationAuditable
    {
        public const int DescriptionLengthMax = 1024;

        protected History()
            : base(null)
        {
        }

        public History(
            string plant,
            string description,
            Guid sourceGuid,
            EventType eventType
        ) : base(plant)
        {
            Description = description;
            SourceGuid = sourceGuid;
            ObjectGuid = sourceGuid;
            ObjectType = "IPO";
            EventType = eventType;
        }

        // private setters needed for Entity Framework
        public string Description { get; private set; }
        public int CreatedById { get; private set; }
        public Guid SourceGuid { get; private set; }
        [Obsolete("Keep for migration only. To be removed in next version")]
        public Guid ObjectGuid { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public EventType EventType { get; private set; }
        public string ObjectType { get; private set; }

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }
    }
}
