using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public interface IFunctionalRoleApiService
    {
        Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByClassificationAsync(string plant, string classification);
    }
}
