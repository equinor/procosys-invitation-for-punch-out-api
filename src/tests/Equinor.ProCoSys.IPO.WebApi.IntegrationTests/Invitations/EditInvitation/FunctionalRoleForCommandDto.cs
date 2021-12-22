using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class FunctionalRoleForCommandDto
    {
        public string Code { get; set; }
        public IList<PersonForCommandDto> Persons { get; set; }
        public int Id { get; set; }
        public string RowVersion { get; set; }
    }
}
