using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql
{
    public class RawSqlRepositoryBase
    {
        //private readonly IConfiguration _configuration;

        //public RawSqlRepositoryBase(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        private readonly IDbConnection _dbConnection;
        public RawSqlRepositoryBase(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string queryString, object parameters)
        {
            //var connectionString = _configuration.GetConnectionString("IPOContext");

            //using (IDbConnection connection = new SqlConnection(connectionString))
            //{

            //}
            return await _dbConnection.QueryAsync<T>(queryString, parameters);
        }
    }
}
