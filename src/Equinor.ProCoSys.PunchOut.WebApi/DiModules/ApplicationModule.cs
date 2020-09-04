using Equinor.Procosys.PunchOut.Domain.Time;
using Equinor.ProCoSys.PunchOut.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.PunchOut.WebApi.DiModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            services.AddDbContext<CallOutContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CallOutContext"));
            });
        }
    }
}
