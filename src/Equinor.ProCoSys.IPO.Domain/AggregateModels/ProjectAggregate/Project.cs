using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate
{
    public class Project : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        public const int NameLengthMax = 30;
        public const int DescriptionLengthMax = 1000;

        protected Project()
            : base(null)
        {
        }

        public Project(string plant, string name, string description, Guid guid)
            : base(plant)
        {
            Name = name;
            Description = description;
            Guid = guid;
        }

        // private setters needed for Entity Framework
        public string Name { get; private set; }
        public string Description { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }
        public Guid Guid { get; private set; } = Guid.Empty;

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }

        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
        }
    }
}
