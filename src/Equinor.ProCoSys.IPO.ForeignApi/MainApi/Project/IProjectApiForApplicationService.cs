using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public interface IProjectApiForApplicationService
    {
        Task<ProCoSysProject> TryGetProjectAsync(string plant, string name, CancellationToken cancellationToken);
    }
}
