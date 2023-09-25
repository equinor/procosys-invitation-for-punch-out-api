using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;

public interface IOutstandingIpoRepository
{
    Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIposByAzureOid(string plant, Guid azureOid);
    Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIposByFunctionalRoleCodes(string plant, IList<string> functionalRoleCodes);
}
