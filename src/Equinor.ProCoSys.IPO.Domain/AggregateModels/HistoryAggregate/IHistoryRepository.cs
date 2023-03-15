using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public interface IHistoryRepository : IRepository<History>
    {
        IList<History> GetHistoryByObjectGuid(Guid guid);
    }
}
