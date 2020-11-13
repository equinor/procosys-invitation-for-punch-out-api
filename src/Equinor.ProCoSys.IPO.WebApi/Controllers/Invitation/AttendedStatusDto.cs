using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class AttendedStatusDto
    {
        public string InvitationRowVersion { get; set; }
        public IEnumerable<ParticipantsToChangeDto> Participants { get; set; }
    }

    public class ParticipantsToChangeDto
    {
        public int ParticipantId { get; set; }
        public bool Attended { get; set; }
        public string RowVersion { get; set; }
    }
}
