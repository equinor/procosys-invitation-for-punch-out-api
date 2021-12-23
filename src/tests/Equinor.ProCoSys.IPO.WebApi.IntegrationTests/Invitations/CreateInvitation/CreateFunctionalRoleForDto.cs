using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation
{
    public class CreateFunctionalRoleForDto
    {
        public string Code { get; set; }
        public IList<CreateInvitedPersonDto> Persons { get; set; }
    }
}
