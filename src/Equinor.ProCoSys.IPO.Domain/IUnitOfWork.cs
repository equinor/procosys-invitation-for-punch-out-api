using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Domain
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        void Commit();
    }
}
