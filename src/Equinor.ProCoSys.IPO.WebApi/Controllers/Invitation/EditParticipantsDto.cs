using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditParticipantsDto
    {
        // Existing participants not included in UpdatedParticipants will be deleted.
        public IEnumerable<EditParticipantDto> UpdatedParticipants { get; set; }
    }
}
