using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Plant
{
    public interface IPlantApiService
    {
        Task<List<ProCoSysPlant>> GetPlantsAsync();
    }
}
