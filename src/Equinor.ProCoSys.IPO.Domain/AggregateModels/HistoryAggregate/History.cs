using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

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
            int objectId,
            ObjectType objectType,
            EventType eventType
        ) : base(plant)
        {
            Description = description;
            ObjectId = objectId;
            ObjectType = objectType;
            EventType = eventType;
        }

        public History(
            string plant,
            string description,
            int objectId,
            IpoRecord preservationRecord
        ) : this(plant, description, objectId, ObjectType.Ipo, EventType.RequirementPreserved)
        {
            if (preservationRecord == null)
            {
                throw new ArgumentNullException(nameof(preservationRecord));
            }
            if (preservationRecord.Plant != plant)
            {
                throw new ArgumentException($"Can't relate item in {preservationRecord.Plant} to item in {plant}");
            }

            IpoRecordId = preservationRecord.Id;
        }

        public string Description { get; private set; }
        public int CreatedById { get; private set; }
        public Guid ObjectGuid { get; private set; }
        public int? IpoRecordId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public EventType EventType { get; private set; }
        public ObjectType ObjectType { get; private set; }
        public Guid? IpoRecordGuid { get; set; }

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
