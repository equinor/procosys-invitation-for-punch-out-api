using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class DapperRepositoryBase
    {
        private readonly IPOContext _context;

        public DapperRepositoryBase(IPOContext context) => _context = context;

        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> queryFunc)
        {
            var connection = _context.Database.GetDbConnection();
            var connectionWasClosed = connection.State != ConnectionState.Open;

            try
            {
                return await queryFunc(connection);
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName} Query timed out. See inner exception for details", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName} SqlException. See inner exception for details.", ex);
            }
            finally
            {
                //If we open it, we have to close it.
                if (connectionWasClosed)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }
    }
}
