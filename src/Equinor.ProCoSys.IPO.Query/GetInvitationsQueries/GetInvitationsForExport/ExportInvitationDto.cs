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
            IList<string> mcPkgs,
            IList<string> commPkgs,
            IList<Participant> participants,
            DateTime completedAtUtc,
            DateTime acceptedAtUtc)
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
            Participants = participants;
            CompletedAtUtc = completedAtUtc;
            AcceptedAtUtc = acceptedAtUtc;
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
        public IList<string> McPkgs { get; }
        public IList<string> CommPkgs { get; }
        public IList<Participant> Participants { get; }
        public DateTime CompletedAtUtc { get; }
        public DateTime AcceptedAtUtc { get; }
    }
}
