using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation
{
    public class ExternalEmailDto
    {
        public int Id { get; set; }
        public string ExternalEmail { get; set; }
        public OutlookResponse? Response { get; set; }
        public string RowVersion { get; set; }
    }
}
