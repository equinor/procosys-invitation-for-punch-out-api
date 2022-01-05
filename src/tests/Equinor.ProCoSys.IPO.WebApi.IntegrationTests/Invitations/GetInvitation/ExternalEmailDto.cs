using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation
{
    public class ExternalEmailDto
    {
        public string ExternalEmail { get; set; }
        public OutlookResponse? Response { get; set; }
    }
}
