using System;
using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation
{
    public class InvitedPersonDto
    {
        public OutlookResponse? Response { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public Guid AzureOid { get; set; }
        public string Email { get; set; }
    }
}
