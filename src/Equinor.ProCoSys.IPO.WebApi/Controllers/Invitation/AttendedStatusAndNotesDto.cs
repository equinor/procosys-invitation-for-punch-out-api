using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class AttendedStatusAndNotesDto
    {
        public string InvitationRowVersion { get; set; }
        public IEnumerable<ParticipantToChangeDto> Participants {get; set;}
    }
}
