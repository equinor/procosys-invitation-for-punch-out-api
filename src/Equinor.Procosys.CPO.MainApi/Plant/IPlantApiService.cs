using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Plant
{
    public interface IPlantApiService
    {
        Task<List<ProcosysPlant>> GetPlantsAsync();
    }
}
