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
    public class CommPkg : PlantEntityBase, ICreationAuditable
    {
        public const int CommPkgNoMaxLength = 30;
        public const int SystemMaxLength = 40;
        private readonly List<Certificate> _certificateScope = new List<Certificate>();

        protected CommPkg()
            : base(null)
        {
        }

        public CommPkg(
            string plant,
            Project project,
            string commPkgNo,
            string description,
            string status,
            string system,
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

            Guid = Guid.NewGuid();
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            System = system;
            ProjectId = project.Id;
            RfocAccepted = false;
            CommPkgGuid = commPkgGuid;
        }

        // private setters needed for Entity Framework
        public Guid Guid { get; private set; }
        public string CommPkgNo { get; private set; }
        public string Description { get; set; }
        public string Status { get; private set; }
        public string System { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public int ProjectId { get; private set; }
        public bool RfocAccepted { get; set; }
        public ICollection<Certificate> CertificateScopes => _certificateScope;
        // TODO: make CommPkgGuid private after FillGuids has completed
        // Guid used across modules.
        // In a system using guid to identify objects between the modules, the guid may identify the object uniquely in one module,
        // but not uniquely in another module due to many-to-many relationship to objects in that distinct module. 
        // E.g. in IPO; one commpkgguid might have multiple invitations.
        // Hence one should always assume that more than one instance exists when such relations are identified.
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

        public void MoveToProject(Project toProject)
        {
            if (toProject is null)
            {
                throw new ArgumentNullException(nameof(toProject));
            }

            ProjectId = toProject.Id;
        }
    }
}
