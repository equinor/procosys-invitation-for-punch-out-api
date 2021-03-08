using Equinor.ProCoSys.IPO.Query.GetInvitations;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class SortingDto
    {
        public SortingDirection Direction { get; set; } = GetInvitationsQuery.DefaultSortingDirection;
        public SortingProperty Property { get; set; } = GetInvitationsQuery.DefaultSortingProperty;
    }
}
