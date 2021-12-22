using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class EditFunctionalRoleForDto
    {
        public string Code { get; set; }
        public IList<EditPersonDto> Persons { get; set; }
        public int Id { get; set; }
        public string RowVersion { get; set; }
    }
}
