using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateInvitationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ProjectName { get; set; }
        public DisciplineType Type { get; set; }
        public IEnumerable<ParticipantDto> Participants { get; set; }
        public IEnumerable<McPkgDto> McPkgScope { get; set; }
        public IEnumerable<CommPkgDto> CommPkgScope { get; set; }
    }
}
