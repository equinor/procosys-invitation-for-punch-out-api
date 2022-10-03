using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class InvitationDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public IpoStatus Status { get; set; }

        public List<ParticipantDto> Participants { get; set; }
    }
}
