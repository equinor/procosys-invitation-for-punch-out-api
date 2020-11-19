using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class McPkg : PlantEntityBase, ICreationAuditable
    {
        protected McPkg()
            : base(null)
        {
        }

        public McPkg(string plant, string projectName, string commPkgNo, string mcPkgNo, string description)
            : base(plant)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentNullException(nameof(projectName));
            }
            if (string.IsNullOrEmpty(commPkgNo))
            {
                throw new ArgumentNullException(nameof(commPkgNo));
            }
            if (string.IsNullOrEmpty(mcPkgNo))
            {
                throw new ArgumentNullException(nameof(mcPkgNo));
            }
            ProjectName = projectName;
            CommPkgNo = commPkgNo;
            Description = description;
            McPkgNo = mcPkgNo;
        }

        public string ProjectName { get; private set; }
        public string CommPkgNo { get; private set; }
        public string Description { get; set; }
        public string McPkgNo { get; }
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
