using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.PunchOut.Database
{
    public class CallOutContext : DbContext
    {
        public CallOutContext(DbContextOptions<CallOutContext> options)
            : base(options)
        {

        }
    }
}
