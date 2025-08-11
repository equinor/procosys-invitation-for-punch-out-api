using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class McPkg : PlantEntityBase, ICreationAuditable
    {
        public const int McPkgNoMaxLength = 30;
        public const int SystemMaxLength = 40;
        private readonly List<Certificate> _certificateScope = new List<Certificate>();

        protected McPkg()
            : base(null)
        {
        }

        public McPkg(
            string plant,
            Project project,
            string commPkgNo,
            string mcPkgNo,
            string description,
            string system,
            Guid mcPkgGuid,
            Guid commPkgGuid)
            : base(plant)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }
            if (string.IsNullOrEmpty(commPkgNo))
            {
                throw new ArgumentNullException(nameof(commPkgNo));
            }
            if (string.IsNullOrEmpty(system))
            {
                throw new ArgumentNullException(nameof(system));
            }
            if (!system.Contains('|') || system.Length < 3)
            {
                throw new ArgumentException($"{(nameof(system))} is not valid. Must be at least three characters and include '|'");
            }
            if (string.IsNullOrEmpty(mcPkgNo))
            {
                throw new ArgumentNullException(nameof(mcPkgNo));
            }

            Guid = Guid.NewGuid();
            ProjectId = project.Id;
            CommPkgNo = commPkgNo;
            System = system;
            Description = description;
            McPkgNo = mcPkgNo;
            RfocAccepted = false;
            McPkgGuid = mcPkgGuid;
            CommPkgGuid = commPkgGuid;
        }

        // private setters needed for Entity Framework
        public Guid Guid { get; private set; }
        public int ProjectId { get; private set; }
        public string CommPkgNo { get; private set; }
        public string Description { get; set; }
        public string McPkgNo { get; private set; }
        public string System { get; private set; }
        public bool RfocAccepted { get; set; }
        public ICollection<Certificate> CertificateScopes => _certificateScope;
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        // TODO: make McPkgGuid private after FillGuids has completed
        public Guid McPkgGuid { get; set; }
        public Guid CommPkgGuid { get; set; }
        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }

        public void MoveToCommPkg(string toCommPkgNo) => CommPkgNo = toCommPkgNo;

        public void Rename(string toMcPkgNo) => McPkgNo = toMcPkgNo;

        public void MoveToProject(Project toProject)
        {
            if (toProject is null)
            {
                throw new ArgumentNullException(nameof(toProject));
            }

            ProjectId = toProject.Id;
        }

        public string SystemSubString => System.Substring(0, System.LastIndexOf('|'));
    }
}
