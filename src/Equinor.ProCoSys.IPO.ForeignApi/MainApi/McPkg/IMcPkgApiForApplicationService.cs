using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public interface IMcPkgApiForApplicationService
    {
        Task<ProCoSysMcPkg> GetMcPkgByIdAsync(string plant, long mcPkgId, CancellationToken cancellationToken);
    }
}
