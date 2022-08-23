using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public interface IHistoryRepository : IRepository<History>
    {
        IList<History> GetHistoryByObjectGuid(Guid guid);
    }
}
