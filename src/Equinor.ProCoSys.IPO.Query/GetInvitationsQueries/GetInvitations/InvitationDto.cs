using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class InvitationDto
    {
        public InvitationDto(
            int id,
            string projectName,
            string title,
            string description,
            string location,
            DisciplineType type,
            IpoStatus status,
            DateTime createdAtUtc,
            int createdById,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            DateTime? completedAtUtc,
            DateTime? acceptedAtUtc,
            string contractorRep,
            string constructionCompanyRep,
            IEnumerable<string> commissioningReps,
            IEnumerable<string> operationReps,
            IEnumerable<string> technicalIntegrityReps,
            IEnumerable<string> supplierReps,
            IEnumerable<string> externalGuests,
            IEnumerable<string> mcPkgNos,
            IEnumerable<string> commPkgNos,
            string rowVersion)
        {
            Id = id;
            ProjectName = projectName;
            Title = title;
            Description = description;
            Location = location;
            Type = type;
            Status = status;
            CreatedAtUtc = createdAtUtc;
            CreatedById = createdById;
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            CompletedAtUtc = completedAtUtc;
            AcceptedAtUtc = acceptedAtUtc;
            ContractorRep = contractorRep;
            ConstructionCompanyRep = constructionCompanyRep;
            CommissioningReps = commissioningReps;
            OperationReps = operationReps;
            TechnicalIntegrityReps = technicalIntegrityReps;
            SupplierReps = supplierReps;
            ExternalGuests = externalGuests;
            McPkgNos = mcPkgNos;
            CommPkgNos = commPkgNos;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string ProjectName { get; }
        public string Title { get; }
        public string Description { get; }
        public string Location { get; }
        public DisciplineType Type { get; }
        public IpoStatus Status { get; }
        public DateTime CreatedAtUtc { get; }
        public int CreatedById { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public DateTime? CompletedAtUtc { get; }
        public DateTime? AcceptedAtUtc { get; }
        public string ContractorRep { get; }
        public string ConstructionCompanyRep { get; }
        public IEnumerable<string> CommissioningReps { get; }
        public IEnumerable<string> OperationReps { get; }
        public IEnumerable<string> TechnicalIntegrityReps { get; }
        public IEnumerable<string> SupplierReps { get; }
        public IEnumerable<string> ExternalGuests { get; }
        public IEnumerable<string> McPkgNos { get; }
        public IEnumerable<string> CommPkgNos { get; }
        public string RowVersion { get; }
    }
}
