using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;

public class OutstandingIpoRepository : DapperRepositoryBase, IOutstandingIpoRepository
{
    public OutstandingIpoRepository(IPOContext context) : base(context) { }

    public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIposByAzureOid(string plant, Guid azureOid)
    {
        var query = OutstandingIpoQuery.CreateQueryFilteredByAzureOid();

        var parameters = new DynamicParameters();
        parameters.Add("@plant", plant);
        parameters.Add("@azureOid", azureOid, DbType.Guid);

        return await QueryAsync<OutstandingIpoDto>(query, parameters);
    }

    public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIposByFunctionalRoleCodes(string plant, IList<string> functionalRoleCodes)
    {
        if (functionalRoleCodes == null)
        {
            throw new ArgumentNullException(nameof(functionalRoleCodes));
        }

        var query = OutstandingIpoQuery.CreateQueryFilteredByFunctionalRole();

        var parameters = new DynamicParameters();
        parameters.Add("@plant", plant);
        parameters.Add("@functionalRoleCodes", functionalRoleCodes);

        return await QueryAsync<OutstandingIpoDto>(query, parameters);
    }
}
