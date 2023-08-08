using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs;

public interface IOutstandingIPOsRawSqlRepository
{
    Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByAzureOid(string plant, Guid azureOid);
    Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByFunctionalRoleCodes(string plant, IList<string> functionalRoleCodes);
}
