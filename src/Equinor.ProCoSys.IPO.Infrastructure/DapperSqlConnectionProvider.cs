using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.Infrastructure
{
    public class DapperSqlConnectionProvider : IDapperSqlConnectionProvider
    {
        private readonly string _dbConnectionString;

        public DapperSqlConnectionProvider(IConfiguration configuration) => _dbConnectionString = configuration.GetConnectionString("IPOContext");

        public DbConnection GetConnection() => new SqlConnection(_dbConnectionString);
    }
}
