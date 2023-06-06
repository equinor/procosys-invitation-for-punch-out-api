using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class HistoryRepository : RepositoryBase<History>, IHistoryRepository
    {
        public HistoryRepository(IPOContext context)
            : base(context, context.History)
        {
        }

        public IList<History> GetHistoryBySourceGuid(Guid sourceGuid) 
            => _context.History.Where(h => h.SourceGuid == sourceGuid).ToList();
    }
}
