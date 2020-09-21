using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.McPkg
{
    public interface IMcPkgApiService
    {
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(string plant, string projectName, string commPkgNo);
    }
}
