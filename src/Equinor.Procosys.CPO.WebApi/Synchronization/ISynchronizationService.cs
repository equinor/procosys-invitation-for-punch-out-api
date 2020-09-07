using System.Threading;
using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.WebApi.Synchronization
{
    public interface ISynchronizationService
    {
        Task Synchronize(CancellationToken cancellationToken);
    }
}
