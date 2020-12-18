using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class IpoRecord : PlantEntityBase, ICreationAuditable
    {
        protected IpoRecord()
            : base(null)
        {
        }

        public IpoRecord(string plant, Person person)
            : base(plant)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }
            IpoAtUtc = TimeService.UtcNow;
            PersonId = person.Id;
            ObjectGuid = Guid.NewGuid();
        }

        public Guid ObjectGuid { get; private set; }
        public DateTime IpoAtUtc { get; private set; }
        public int PersonId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }

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
