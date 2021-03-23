using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries
{
    public class InvitationForQueryDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DisciplineType Type { get; set; }
        public IpoStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int CreatedById { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? AcceptedAtUtc { get; set; }
        public string RowVersion { get; set; }
    }
}
