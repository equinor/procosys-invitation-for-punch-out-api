using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Permission
{
    public interface IPermissionApiService
    {
        Task<IList<string>> GetPermissionsAsync(string plantId);
        Task<IList<string>> GetProjectsAsync(string plantId);
        Task<IList<string>> GetContentRestrictionsAsync(string plantId);
    }
}
