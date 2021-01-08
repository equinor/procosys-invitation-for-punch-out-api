using System;

namespace Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs
{
    public class CommPkgsWithMdpIposDto
    {
        public CommPkgsWithMdpIposDto(
            string commPkgNo,
            int latestMdpInvitationId,
            DateTime createdAtUtc,
            bool isAccepted)
        {
            CommPkgNo = commPkgNo;
            LatestMdpInvitationId = latestMdpInvitationId;
            CreatedAtUtc = createdAtUtc;
            IsAccepted = isAccepted;
        }
        public string CommPkgNo { get; }
        public int LatestMdpInvitationId { get; }
        public DateTime CreatedAtUtc { get; }
        public bool IsAccepted { get; }
    }
}
