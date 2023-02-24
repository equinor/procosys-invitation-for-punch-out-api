using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Permission
{
    public interface IPermissionApiService
    {
        Task<List<ProCoSysPlant>> GetAllPlantsForUserAsync(Guid azureOid);
        Task<List<string>> GetPermissionsAsync(string plantId);
        Task<List<ProCoSysProject>> GetAllOpenProjectsAsync(string plantId);
        Task<List<string>> GetContentRestrictionsAsync(string plantId);
    }
}
