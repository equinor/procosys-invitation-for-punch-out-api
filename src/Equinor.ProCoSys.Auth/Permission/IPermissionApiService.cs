using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Permission
{
    public interface IPermissionApiService
    {
        Task<List<AccessablePlant>> GetAllPlantsForUserAsync(Guid azureOid);
        Task<List<string>> GetPermissionsForCurrentUserAsync(string plantId);
        Task<List<AccessableProject>> GetAllOpenProjectsForCurrentUserAsync(string plantId);
        Task<List<string>> GetContentRestrictionsForCurrentUserAsync(string plantId);
    }
}
