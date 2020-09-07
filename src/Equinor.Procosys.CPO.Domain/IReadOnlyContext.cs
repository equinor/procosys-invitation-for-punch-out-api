using System.Linq;

namespace Equinor.Procosys.CPO.Domain
{
    public interface IReadOnlyContext
    {
        IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class;
    }
}
