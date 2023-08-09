using System.Data.Common;
using Equinor.ProCoSys.IPO.Infrastructure;
using Microsoft.Data.Sqlite;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public class DapperSqlLiteConnectionProvider : IDapperSqlConnectionProvider
    {
        private readonly SqliteConnection _dbConnection;

        public DapperSqlLiteConnectionProvider(SqliteConnection dbConnection) => _dbConnection = dbConnection;

        public DbConnection GetConnection() => _dbConnection;
    }
}
