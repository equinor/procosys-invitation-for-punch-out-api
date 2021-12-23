using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditFunctionalRoleDto
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public IEnumerable<EditInvitedPersonDto> Persons { get; set; }
        public string RowVersion { get; set; }
    }
}
