using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public interface ILibraryFunctionalRoleApiService
    {
        Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByClassificationAsync(string plant, string classification);
        Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByCodeAsync(string plant, List<string> functionalRoleCodes);
    }
}
