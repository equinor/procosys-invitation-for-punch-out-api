using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public interface ICommPkgApiService
    {
        Task<IList<ProCoSysCommPkg>> SearchCommPkgsByCommPkgNoAsync(string plant, int projectId, string startsWithCommPkgNo);
        Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(string plant, string projectName, IList<string> commPkgNos);
    }
}
