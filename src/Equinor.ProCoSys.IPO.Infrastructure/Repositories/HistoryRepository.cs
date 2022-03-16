using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class HistoryRepository : RepositoryBase<History>, IHistoryRepository
    {
        public HistoryRepository(IPOContext context)
            : base(context, context.History)
        {
        }

        public void RemoveHistory(History history)
            => _context.History.Remove(history);
    }
}
