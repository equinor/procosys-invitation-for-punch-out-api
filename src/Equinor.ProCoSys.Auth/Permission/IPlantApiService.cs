using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Permission
{
    public interface IPlantApiService
    {
        Task<List<ProCoSysPlant>> GetAllPlantsAsync();
    }
}
