using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Meeting
{
    public class CreateMeetingDto
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<Guid> ParticipantOids { get; set; }
    }
}
