using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.FunctionalRole
{
    public interface IMainFunctionalRoleApiService
    {
        Task<IList<string>> GetFunctionalRoleCodesByPersonOidAsync(string plant, string azureOid);
    }
}
