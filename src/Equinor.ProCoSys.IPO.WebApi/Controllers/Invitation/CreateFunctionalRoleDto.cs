using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateFunctionalRoleDto
    {
        public string Code { get; set; }
        public IEnumerable<CreateInvitedPersonDto> Persons { get; set; }
    }
}
