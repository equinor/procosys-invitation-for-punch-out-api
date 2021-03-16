using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class InvitationsResult
    {
        public InvitationsResult(int maxAvailable, IEnumerable<InvitationForQueryDto> invitations)
        {
            MaxAvailable = maxAvailable;
            Invitations = invitations ?? new List<InvitationForQueryDto>();
        }

        public int MaxAvailable { get; }
        public IEnumerable<InvitationForQueryDto> Invitations { get; }
    }
}
