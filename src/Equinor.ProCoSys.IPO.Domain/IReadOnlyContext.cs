using System.Linq;

namespace Equinor.ProCoSys.IPO.Domain
{
    public interface IReadOnlyContext
    {
        IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class;
    }
}
