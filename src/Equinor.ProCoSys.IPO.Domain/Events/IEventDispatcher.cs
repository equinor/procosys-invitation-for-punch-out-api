using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public interface IEventDispatcher
    {
        Task DispatchPreSaveAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default);
        Task DispatchPostSaveAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default);
    }
}
