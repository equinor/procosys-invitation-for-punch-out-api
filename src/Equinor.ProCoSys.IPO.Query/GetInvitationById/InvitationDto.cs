using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class InvitationDto
    {
        public InvitationDto(
            string projectName,
            string title,
            string description,
            string location,
            DisciplineType type,
            IpoStatus status,
            PersonDto createdBy,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            string rowVersion)
        {
            ProjectName = projectName;
            Title = title;
            Description = description;
            Location = location;
            Type = type;
            Status = status;
            CreatedBy = createdBy;
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            RowVersion = rowVersion;
        }

        public string ProjectName { get; }
        public string Title { get; }
        public string Description { get; }
        public string Location { get; }
        public DisciplineType Type { get; }
        public IpoStatus Status { get; }
        public PersonDto CreatedBy { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public string RowVersion { get; }
        public IEnumerable<ParticipantDto> Participants { get; set; }
        public IEnumerable<McPkgScopeDto> McPkgScope { get; set; }
        public IEnumerable<CommPkgScopeDto> CommPkgScope { get; set; }
    }
}
