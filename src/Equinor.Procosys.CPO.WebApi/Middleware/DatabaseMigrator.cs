using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.CPO.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Equinor.Procosys.CPO.WebApi.Middleware
{
    public class DatabaseMigrator : IHostedService
    {
        private readonly IServiceScopeFactory _serviceProvider;

        public DatabaseMigrator(IServiceScopeFactory serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CPOContext>();
                await dbContext.Database.MigrateAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
