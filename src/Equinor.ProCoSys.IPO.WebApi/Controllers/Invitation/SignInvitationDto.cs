using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class SignInvitationDto
    {
        public IEnumerable<ParticipantWhenSigningDto> Participants { get; set; }
    }
}
