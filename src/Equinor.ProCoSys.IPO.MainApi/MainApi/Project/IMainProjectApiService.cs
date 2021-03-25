using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public interface IMainProjectApiService
    {
        Task<ProCoSysProject> TryGetProjectAsync(string plant, string name);
        Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant);
    }
}
