using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate
{
    public class Certificate : PlantEntityBase, IAggregateRoot, ICreationAuditable
    {
        private readonly List<McPkg> _certificateMcPkgScope = new List<McPkg>();
        private readonly List<CommPkg> _certificateCommPkgScope = new List<CommPkg>();

        protected Certificate()
            : base(null)
        {
        }

        public Certificate(
            string plant,
            Project project,
            Guid pcsGuid)
            : base(plant)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }
          
            ProjectId = project.Id;
            PcsGuid = pcsGuid;
        }

        // private setters needed for Entity Framework
        public int ProjectId { get; private set; }
        public Guid PcsGuid { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public IReadOnlyCollection<McPkg> CertificateMcPkgs => _certificateMcPkgScope.AsReadOnly();
        public IReadOnlyCollection<CommPkg> CertificateCommPkgs => _certificateCommPkgScope.AsReadOnly();

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }

        public void AddCommPkgRelation(CommPkg commPkg)
        {
            if (commPkg == null)
            {
                throw new ArgumentNullException(nameof(commPkg));
            }

            if (commPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {commPkg.Plant} to item in {Plant}");
            }

            if (commPkg.ProjectId != ProjectId)
            {
                throw new ArgumentException($"Can't relate item in project {commPkg.ProjectId} to item in project {ProjectId}");
            }

            _certificateCommPkgScope.Add(commPkg);
        }

        public void AddMcPkgRelation(McPkg mcPkg)
        {
            if (mcPkg == null)
            {
                throw new ArgumentNullException(nameof(mcPkg));
            }

            if (mcPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {mcPkg.Plant} to item in {Plant}");
            }

            if (mcPkg.ProjectId != ProjectId)
            {
                throw new ArgumentException($"Can't relate item in project {mcPkg.ProjectId} to item in project {ProjectId}");
            }

            _certificateMcPkgScope.Add(mcPkg);
        }
    }
}
