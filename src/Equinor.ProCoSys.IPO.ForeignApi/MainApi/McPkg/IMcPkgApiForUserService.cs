using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public interface IMcPkgApiForUserService
    {
        Task<IList<ProCoSysMcPkgOnCommPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(string plant, string projectName, string commPkgNo, CancellationToken cancellationToken);
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(string plant, string projectName, IList<string> mcPkgNos, CancellationToken cancellationToken);
    }
}
