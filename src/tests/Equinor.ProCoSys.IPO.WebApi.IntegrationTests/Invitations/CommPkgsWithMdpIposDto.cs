using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class CommPkgsWithMdpIposDto
    {
        public string CommPkgNo { get; set; }
        public int LatestMdpInvitationId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsAccepted { get; set; }
    }
}
