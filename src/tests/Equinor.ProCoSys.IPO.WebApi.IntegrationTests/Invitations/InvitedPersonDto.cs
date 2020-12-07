using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class InvitedPersonDto
    {
        public PersonDto Person { get; set; }
        public OutlookResponse? Response { get; set; }
        public bool Required { get; set; }
    }
}
