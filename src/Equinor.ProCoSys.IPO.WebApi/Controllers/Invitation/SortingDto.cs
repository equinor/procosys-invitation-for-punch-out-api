using Equinor.ProCoSys.IPO.Query.GetInvitations;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class SortingDto
    {
        public SortingDirection Direction { get; set; } = GetInvitationsQuery.DefaultSortingDirection;
        public SortingProperty Property { get; set; } = GetInvitationsQuery.DefaultSortingProperty;
    }
}
