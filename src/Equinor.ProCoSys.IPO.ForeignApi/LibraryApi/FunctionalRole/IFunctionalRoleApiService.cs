using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public interface IFunctionalRoleApiService
    {
        Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByClassificationAsync(string plant, string classification, CancellationToken cancellationToken);
        Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByCodeAsync(string plant, List<string> functionalRoleCodes, CancellationToken cancellationToken);
    }
}
