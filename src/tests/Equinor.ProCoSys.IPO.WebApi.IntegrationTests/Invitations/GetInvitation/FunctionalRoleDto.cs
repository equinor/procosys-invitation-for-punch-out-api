using System.Collections.Generic;
using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation
{
    public class FunctionalRoleDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public IEnumerable<InvitedPersonDto> Persons { get; set; }
        public OutlookResponse? Response { get; set; }
        public string RowVersion { get; set; }
    }
}
