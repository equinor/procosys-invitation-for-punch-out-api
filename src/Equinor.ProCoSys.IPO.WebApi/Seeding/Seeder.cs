using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.IPO.WebApi.Seeding
{
    public class Seeder : IHostedService
    {
        private static readonly Person s_seederUser = new Person(new Guid("12345678-1234-1234-1234-123456789123"), "Angus", "MacGyver");
        private readonly IServiceScopeFactory _serviceProvider;

        public Seeder(IServiceScopeFactory serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var plantProvider = new SeedingPlantProvider("PCS$SEED");

                using (var dbContext = new IPOContext(
                    scope.ServiceProvider.GetRequiredService<DbContextOptions<IPOContext>>(),
                    plantProvider,
                    scope.ServiceProvider.GetRequiredService<IEventDispatcher>(),
                    new SeederUserProvider()))
                {
                    // If the seeder user exists in the database, it's already been seeded. Don't seed again.
                    if (await dbContext.Persons.AnyAsync(p => p.Oid == s_seederUser.Oid))
                    {
                        return;
                    }

                    /* 
                     * Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
                     * This is the first user that is added to the database and will not get "Created" and "CreatedBy" data.
                     */
                    dbContext.Persons.Add(s_seederUser);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    var personRepository = new PersonRepository(dbContext);

                    personRepository.AddUsers(250);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private class SeederUserProvider : ICurrentUserProvider
        {
            public Guid GetCurrentUserOid() => s_seederUser.Oid;
            public Guid? TryGetCurrentUserOid() => s_seederUser.Oid;
            public bool IsCurrentUserAuthenticated() => false;
            public ClaimsPrincipal GetCurrentUser() => new ClaimsPrincipal();
        }
    }
}
