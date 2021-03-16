using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class ExportDto
    {
        public ExportDto(IList<ExportInvitationDto> invitations, UsedFilterDto usedFilter)
        {
            UsedFilter = usedFilter;
            Invitations = invitations ?? new List<ExportInvitationDto>();
        }

        public IList<ExportInvitationDto> Invitations { get; }
        public UsedFilterDto UsedFilter { get; }
    }
}
