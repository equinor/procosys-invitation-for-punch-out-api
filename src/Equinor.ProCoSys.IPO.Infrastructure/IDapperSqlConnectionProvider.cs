using System.Data.Common;

namespace Equinor.ProCoSys.IPO.Infrastructure
{
    public interface IDapperSqlConnectionProvider
    {
        public DbConnection GetConnection();
    }
}
