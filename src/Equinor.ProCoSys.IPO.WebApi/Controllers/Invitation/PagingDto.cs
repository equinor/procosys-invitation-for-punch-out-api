using Equinor.ProCoSys.IPO.Query.GetInvitations;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class PagingDto
    {
        public int Page { get; set; } = GetInvitationsQuery.DefaultPage;
        public int Size { get; set; } = GetInvitationsQuery.DefaultPagingSize;
    }
}
