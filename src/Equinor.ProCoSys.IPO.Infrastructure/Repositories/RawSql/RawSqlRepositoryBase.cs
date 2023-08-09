using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql
{
    public class RawSqlRepositoryBase
    {
        private readonly IDapperSqlConnectionProvider _dapperSqlConnectionProvider;

        public RawSqlRepositoryBase(IDapperSqlConnectionProvider dapperSqlConnectionProvider) => _dapperSqlConnectionProvider = dapperSqlConnectionProvider;

        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                var connection = _dapperSqlConnectionProvider.GetConnection();
                return await getData(connection);
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
