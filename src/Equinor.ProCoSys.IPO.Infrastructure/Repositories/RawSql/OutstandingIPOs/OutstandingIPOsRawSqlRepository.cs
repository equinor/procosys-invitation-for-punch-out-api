using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs
{
    public class OutstandingIPOsRawSqlRepository : RawSqlRepositoryBase, IOutstandingIPOsRawSqlRepository
    {
        public OutstandingIPOsRawSqlRepository(IDbConnection dbConnection) : base(dbConnection)
        {
        }

        public async Task<IEnumerable<OutstandingIpoDto>> GetOutstandingIPOsByAzureOid(string plant, Guid azureOid)
        {
            var query = OutstandingIPOsQuery.CreateAzureOidQuery();
            var parameters = new DynamicParameters();
            parameters.Add("@plant", plant);
            parameters.Add("@azureOid", azureOid, DbType.Guid);

            var result = await QueryAsync<OutstandingIpoDto>(query, parameters);

            return result;
        }

        public async Task<bool> ExistsAnyOutstandingIPOsWithFunctionalRoleCodes(string plant)
        {
            var query = OutstandingIPOsQuery.CreateExistsFunctionalRoleQuery();
            var parameters = new DynamicParameters();
            parameters.Add("@plant", plant);

            var result = await ExecuteScalarAsync<int>(query, parameters);

            return result != null && result > 0;
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

            var result = await QueryAsync<OutstandingIpoDto>(query, parameters);

            return result;
        }
    }
}
