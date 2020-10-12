using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateInvitationDto
    {
        public CreateMeetingDto Meeting { get; set; }
    }

    public class CreateMeetingDto
    {
        public string Title { get; set; }
        public string BodyHtml { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IEnumerable<Guid> RequiredParticipantOids { get; set; }
        public IEnumerable<string> RequiredParticipantEmails { get; set; }
        public IEnumerable<Guid> OptionalParticipantOids { get; set; }
        public IEnumerable<string> OptionalParticipantEmails { get; set; }

    }
}
