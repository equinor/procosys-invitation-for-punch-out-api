using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo
{
    public class InvitationForMainDto
    {
        public InvitationForMainDto(
            int id,
            string title,
            string description,
            DisciplineType type,
            IpoStatus status,
            DateTime? completedAtUtc,
            DateTime? acceptedAtUtc,
            DateTime meetingTimeUtc,
            string rowVersion)
        {
            Id = id;
            Title = title;
            Description = description;
            Type = type;
            Status = status;
            CompletedAtUtc = completedAtUtc;
            AcceptedAtUtc = acceptedAtUtc;
            MeetingTimeUtc = meetingTimeUtc;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public DisciplineType Type { get; }
        public IpoStatus Status { get; }
        public DateTime? CompletedAtUtc { get; }
        public DateTime? AcceptedAtUtc { get; }
        public DateTime MeetingTimeUtc { get; }
        public string RowVersion { get; }
    }
}
