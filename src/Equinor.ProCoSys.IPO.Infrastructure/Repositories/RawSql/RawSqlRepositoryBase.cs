using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql
{
    public class RawSqlRepositoryBase
    {
        private readonly IDbConnection _dbConnection;
        public RawSqlRepositoryBase(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string queryString, DynamicParameters parameters) => await _dbConnection.QueryAsync<T>(queryString, parameters);

        public async Task<T> ExecuteScalarAsync<T>(string queryString, DynamicParameters parameters) => await _dbConnection.ExecuteScalarAsync<T>(queryString, parameters);
    }
}
