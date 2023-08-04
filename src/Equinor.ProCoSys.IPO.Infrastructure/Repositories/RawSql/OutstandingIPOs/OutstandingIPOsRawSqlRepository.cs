using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs
{
    public class OutstandingIPOsRawSqlRepository : RawSqlRepositoryBase, IOutstandingIPOsRawSqlRepository
    {
        //public OutstandingIPOsRawSqlRepository(IConfiguration configuration) : base(configuration)
        //{

        //}
        public OutstandingIPOsRawSqlRepository(IDbConnection dbConnection) : base(dbConnection)
        {

        }

        public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByAzureOid(string plant, Guid azureOid)
        {
            var query = OutstandingIPOsQuery.CreateAzureOidQuery();

            var result = await QueryAsync<OutstandingIpoDto>(query, new {plant, azureOid});

            return result;
        }

        public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByFunctionalRoleCodes(string plant, IList<string> functionalRoleCodes)
        {
            var query = OutstandingIPOsQuery.CreateFunctionalRoleQuery();

            var result = await QueryAsync<OutstandingIpoDto>(query, new { plant, functionalRoleCodes });

            return result;
        }
    }
}
