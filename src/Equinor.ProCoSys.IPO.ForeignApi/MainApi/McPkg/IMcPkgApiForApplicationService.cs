using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public interface IMcPkgApiForApplicationService
    {
        Task<ProCoSysMcPkg> GetMcPkgByIdAsync(string plant, long mcPkgId, CancellationToken cancellationToken);
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(
            string plant,
            string projectName,
            IList<string> mcPkgNos,
            CancellationToken cancellationToken);
        Task SetM01DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos,
            CancellationToken cancellationToken);
    }
}
