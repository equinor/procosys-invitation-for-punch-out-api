using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class InvitationsResult
    {
        public InvitationsResult(int maxAvailable, IEnumerable<InvitationDto> invitations)
        {
            MaxAvailable = maxAvailable;
            Invitations = invitations ?? new List<InvitationDto>();
        }

        public int MaxAvailable { get; }
        public IEnumerable<InvitationDto> Invitations { get; }
    }
}
