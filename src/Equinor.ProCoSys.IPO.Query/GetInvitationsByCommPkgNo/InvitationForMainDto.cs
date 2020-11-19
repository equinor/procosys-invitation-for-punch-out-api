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
            string rowVersion)
        {
            Id = id;
            Title = title;
            Description = description;
            Type = type;
            Status = status;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public DisciplineType Type { get; }
        public IpoStatus Status { get; }
        public string RowVersion { get; }
        public DateTime MeetingTimeUtc { get; set; }
    }
}
