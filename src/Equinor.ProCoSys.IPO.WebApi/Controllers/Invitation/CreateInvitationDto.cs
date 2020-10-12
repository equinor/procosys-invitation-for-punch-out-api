using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateInvitationDto
    {
        public CreateMeetingDto Meeting { get; set; }
        public string Title { get; set; }
        public string ProjectName { get; set; }
        public string Type { get; set; }
        //public IEnumerable<ParticipantDto> Participants { get; set; }
        public IEnumerable<McPkgDto> McPkgScope { get; set; }
        public IEnumerable<CommPkgDto> CommPkgScope { get; set; }
    }

    public class CreateMeetingDto
    {
        public string Title { get; set; }
        public string BodyHtml { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IEnumerable<Guid> ParticipantOids { get; set; }
    }
}
