using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;

public interface IExportIpoRepository
{
    Task<List<Invitation>> GetInvitationsWithIncludesAsync(List<int> invitationIds, IPlantProvider plantProvider,
        CancellationToken cancellationToken);
}

