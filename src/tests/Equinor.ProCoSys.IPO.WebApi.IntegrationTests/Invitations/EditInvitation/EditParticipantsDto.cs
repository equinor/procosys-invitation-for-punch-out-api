using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class EditParticipantsDto
    {
        public IEnumerable<EditParticipantDto> UpdatedParticipants { get; set; }
    }
}
