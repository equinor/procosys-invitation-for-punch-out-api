using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant
{
    public interface IPlantApiService
    {
        Task<List<ProCoSysPlant>> GetPlantsAsync();
    }
}
