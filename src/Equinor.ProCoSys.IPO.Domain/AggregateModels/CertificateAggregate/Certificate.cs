﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate
{
    public class Certificate : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
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
            if (project.Plant != plant)
            {
                throw new ArgumentException($"Can't relate {nameof(project)} in {project.Plant} to item in {plant}");
            }

            ProjectId = project.Id;
            PcsGuid = pcsGuid;
        }

        // private setters needed for Entity Framework
        public int ProjectId { get; private set; }
        public Guid PcsGuid { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }
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

        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
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

            _certificateMcPkgScope.Add(mcPkg);
        }
    }
}