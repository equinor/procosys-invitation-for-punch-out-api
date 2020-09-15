using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Project
{
    public interface IProjectApiService
    {
        Task<ProCoSysProject> TryGetProjectAsync(string plant, string name);
        Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant);
    }
}
