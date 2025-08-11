using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CompletePunchOutDto
    {
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
        public IEnumerable<ParticipantToChangeDto> Participants { get; set; }
    }
}
