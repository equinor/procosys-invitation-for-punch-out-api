using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class AcceptPunchOutDto
    {
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
        public IEnumerable<ParticipantToUpdateNoteDto> Participants { get; set; }
    }
}
