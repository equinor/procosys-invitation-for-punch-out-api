using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class ExportInvitationDto
    {
        public ExportInvitationDto(
            int id,
            string projectName,
            IpoStatus status,
            string title,
            string description,
            string type,
            string location,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            IEnumerable<string> mcPkgs,
            IEnumerable<string> commPkgs,
            string contractorRep,
            string constructionCompanyRep,
            DateTime? completedAtUtc,
            DateTime? acceptedAtUtc,
            DateTime createdAtUtc,
            string createdBy)
        {
            Id = id;
            ProjectName = projectName;
            Status = status;
            Title = title;
            Description = description;
            Type = type;
            Location = location;
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            McPkgs = mcPkgs;
            CommPkgs = commPkgs;
            ContractorRep = contractorRep;
            ConstructionCompanyRep = constructionCompanyRep;
            CompletedAtUtc = completedAtUtc;
            AcceptedAtUtc = acceptedAtUtc;
            CreatedAtUtc = createdAtUtc;
            CreatedBy = createdBy;

            Participants = new List<ExportParticipantDto>();
            History = new List<ExportHistoryDto>();
        }
        public int Id { get; }
        public string ProjectName { get; }
        public IpoStatus Status { get; }
        public string Title { get; }
        public string Description { get; }
        public string Type { get; }
        public string Location { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public IEnumerable<string> McPkgs { get; }
        public IEnumerable<string> CommPkgs { get; }
        public string ContractorRep { get; }
        public string ConstructionCompanyRep { get; }
        public DateTime? CompletedAtUtc { get; }
        public DateTime? AcceptedAtUtc { get; }
        public DateTime CreatedAtUtc { get; }
        public string CreatedBy { get; }
        public List<ExportParticipantDto> Participants { get; }
        public List<ExportHistoryDto> History { get; }
    }
}
