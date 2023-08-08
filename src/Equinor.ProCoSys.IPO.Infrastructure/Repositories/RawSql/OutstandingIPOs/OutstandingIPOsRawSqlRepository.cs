using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs
{
    public class OutstandingIPOsRawSqlRepository : RawSqlRepositoryBase, IOutstandingIPOsRawSqlRepository
    {
        public OutstandingIPOsRawSqlRepository(IConfiguration configuration) : base(configuration)
        {
          
        }

        public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByAzureOid(string plant, Guid azureOid)
        {
            var query = OutstandingIPOsQuery.CreateAzureOidQuery();
            var parameters = new DynamicParameters();
            parameters.Add("@plant", plant);
            parameters.Add("@azureOid", azureOid, DbType.Guid);

            return await WithConnection(async c =>
            {
                var result = await c.QueryAsync<OutstandingIpoDto>(query, parameters);
                return result;
            });

          
        }

        public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByFunctionalRoleCodes(string plant, IList<string> functionalRoleCodes)
        {
            if (functionalRoleCodes == null)
            {
                throw new ArgumentNullException(nameof(functionalRoleCodes));
            }

            var query = OutstandingIPOsQuery.CreateFunctionalRoleQuery();
            var parameters = new DynamicParameters();
            parameters.Add("@plant", plant);
            parameters.Add("@functionalRoleCodes", functionalRoleCodes);

            return await WithConnection(async c =>
            {
                var result = await c.QueryAsync<OutstandingIpoDto>(query, parameters);
                return result;
            });
        }
    }
}
