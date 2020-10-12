using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class ParticipantDto
    {
        public string Organization { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public int SortKey { get; set; }
        public IEnumerable<PersonDto> Person { get; set; }
        public IEnumerable<FunctionalRoleDto> FunctionalRole { get; set; }
    }
}
