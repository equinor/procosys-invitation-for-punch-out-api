using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class InvitationResultsDto
    {
        public int MaxAvailable { get; set; }
        public IList<Query.GetInvitationsQueries.InvitationForQueryDto> Invitations { get; set; }
}
}
