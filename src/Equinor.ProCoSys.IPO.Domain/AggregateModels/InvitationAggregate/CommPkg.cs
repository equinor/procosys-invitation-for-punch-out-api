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

        public CommPkg(
            string plant,
            string projectName,
            string commPkgNo,
            string description,
            string status,
            string system)
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
            if (string.IsNullOrEmpty(system))
            {
                throw new ArgumentNullException(nameof(system));
            }
            ProjectName = projectName;
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            System = system;
        }

        public string ProjectName { get; private set; }
        public string CommPkgNo { get; private set; }
        public string Description { get; set; }
        public string Status { get; private set; }
        public string System { get; set; }
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

        public void MoveToProject(string toProject) => ProjectName = toProject;
    }
}
