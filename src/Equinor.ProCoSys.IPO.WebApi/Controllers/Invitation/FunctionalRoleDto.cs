using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FunctionalRoleDto
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public bool UsePersonalEmail { get; set; }
        public IEnumerable<PersonDto> Persons { get; set; }
    }
}
