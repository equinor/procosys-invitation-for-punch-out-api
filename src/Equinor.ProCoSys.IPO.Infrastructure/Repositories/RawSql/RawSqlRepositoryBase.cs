using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql
{
    public class RawSqlRepositoryBase
    {
        private readonly string _dbConnectionString;

        public RawSqlRepositoryBase(IConfiguration configuration)
        {
            _dbConnectionString = configuration.GetConnectionString("IPOContext");
        }

        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_dbConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName} Query timed out. See inner exception for details", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName} SqlException. See inner exception for details.", ex);
            }
        }
    }
}
