using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public interface IProjectApiForUsersService
    {
        Task<ProCoSysProject> TryGetProjectAsync(string plant, string name, CancellationToken cancellationToken);
        Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant);
    }
}
