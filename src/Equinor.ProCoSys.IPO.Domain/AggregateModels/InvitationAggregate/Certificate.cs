using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
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
        public Guid PcsGuid { get; private set; } // Lag index på denne
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

        //private void SetScope(IList<CertificateScope> scopes)
        //{
        //    if (scopes.Count > 0)
        //    {
        //        foreach (var scope in scopes)
        //        {
        //            _certificateScope.Add(scope);
        //        }
        //    }
        //}
    }
}
