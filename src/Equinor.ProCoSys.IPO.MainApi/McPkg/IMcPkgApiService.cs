using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.McPkg
{
    public interface IMcPkgApiService
    {
        Task<IList<ProCoSysMcPkg>> SearchMcPkgsByMcPkgNoAsync(string plant, int projectId, string startsWithMcPkgNo);
    }
}
