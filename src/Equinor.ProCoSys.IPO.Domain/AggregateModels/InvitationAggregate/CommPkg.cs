using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class CommPkg : PlantEntityBase, ICreationAuditable
    {
        protected CommPkg()
            : base(null)
        {
        }

        public CommPkg(string plant, string projectName, string commPkgNo, string description, string status)
            : base(plant)
        {
            ProjectName = projectName;
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
        }

        public string ProjectName { get; private set; }
        public string CommPkgNo { get; private set; }
        public string Description { get; set; }
        public string Status { get; private set; }
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
