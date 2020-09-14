using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.CommPkg
{
    public interface ICommPkgApiService
    {
        Task<IList<ProcosysCommPkg>> SearchCommPkgsByCommPkgNoAsync(string plant, int projectId, string startsWithTagNo);
    }
}
