using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CompleteInvitationDto
    {
        public string InvitationRowVersion { get; set; }
        public string ContractorRowVersion { get; set; }
        public IEnumerable<ParticipantToChangeDto> Participants { get; set; }
    }
}
