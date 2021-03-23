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
            IList<string> mcPkgNos,
            IList<string> commPkgNos,
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
        public IList<string> McPkgNos { get; }
        public IList<string> CommPkgNos { get; }
        public string RowVersion { get; }
    }
}
