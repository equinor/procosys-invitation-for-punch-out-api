using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public interface ICommPkgApiForUserService
    {
        Task<ProCoSysCommPkgSearchResult> SearchCommPkgsByCommPkgNoAsync(
            string plant,
            string projectName,
            string startsWithCommPkgNo,
            CancellationToken cancellationToken,
            int? itemsPerPage = 10,
            int? currentPage = 0);

        Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(
            string plant,
            string projectName,
            IList<string> commPkgNos,
            CancellationToken cancellationToken);

        Task<IList<ProCoSysRfocOnCommPkg>> GetRfocGuidsByCommPkgNosAsync(
            string plant,
            string projectName,
            IList<string> commPkgNos,
            CancellationToken cancellationToken);
    }
}
