using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public interface ICommPkgApiForApplicationService
    {
        Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(
            string plant,
            string projectName,
            IList<string> commPkgNos,
            CancellationToken cancellationToken);
    }
}
