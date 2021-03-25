using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public interface IMainCommPkgApiService
    {
        Task<ProCoSysCommPkgSearchResult> SearchCommPkgsByCommPkgNoAsync(string plant, string projectName, string startsWithCommPkgNo, int? itemsPerPage = 10, int? currentPage = 0);
        Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(string plant, string projectName, IList<string> commPkgNos);
    }
}
