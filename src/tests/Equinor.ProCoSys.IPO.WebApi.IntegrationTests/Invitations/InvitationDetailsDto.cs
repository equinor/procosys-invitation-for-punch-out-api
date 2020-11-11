using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class InvitationDetailsDto
    {
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DisciplineType Type { get; set; }
        public IpoStatus Status { get; set; }
        public string RowVersion { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}
