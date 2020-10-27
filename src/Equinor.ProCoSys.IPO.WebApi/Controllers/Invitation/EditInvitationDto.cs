using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditInvitationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ProjectName { get; set; }
        public DisciplineType Type { get; set; }
        // Existing participants not included in UpdatedParticipants will be deleted.
        public IEnumerable<ParticipantDto> UpdatedParticipants { get; set; }
        public IEnumerable<ParticipantDto> NewParticipants { get; set; }
        // Existing mc pkgs not included in UpdatedMcPkgScope will be deleted.
        public IEnumerable<McPkgDto> UpdatedMcPkgScope { get; set; }
        public IEnumerable<McPkgDto> NewMcPkgScope { get; set; }
        // Existing comm pkgs not included in UpdatedCommPkgScope will be deleted.
        public IEnumerable<CommPkgDto> UpdatedCommPkgScope { get; set; }
        public IEnumerable<CommPkgDto> NewCommPkgScope { get; set; }
    }
}
