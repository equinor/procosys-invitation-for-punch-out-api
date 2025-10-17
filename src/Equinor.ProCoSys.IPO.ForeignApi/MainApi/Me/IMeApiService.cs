using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me
{
    public interface IMeApiService
    {
        Task<IList<string>> GetFunctionalRoleCodesAsync(string plant, CancellationToken cancellationToken);
    }
}
