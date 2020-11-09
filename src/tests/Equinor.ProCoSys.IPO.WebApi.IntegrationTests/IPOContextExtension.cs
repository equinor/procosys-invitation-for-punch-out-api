using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public static class IPOContextExtension
    {
        private static string _seederOid = "00000000-0000-0000-0000-999999999999";

        public static void CreateNewDatabaseWithCorrectSchema(this IPOContext dbContext)
        {
            var migrations = dbContext.Database.GetPendingMigrations();
            if (migrations.Any())
            {
                dbContext.Database.Migrate();
            }
        }

        public static void Seed(this IPOContext dbContext, IServiceProvider serviceProvider)
        {
            var userProvider = serviceProvider.GetRequiredService<CurrentUserProvider>();
            var plantProvider = serviceProvider.GetRequiredService<PlantProvider>();
            userProvider.SetCurrentUserOid(new Guid(_seederOid));
            plantProvider.SetPlant(SeedingData.Plant);
            
            /* 
             * Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
             * This is the first user that is added to the database and will not get "Created" and "CreatedBy" data.
             */
            SeedCurrentUserAsPerson(dbContext, userProvider);

            var plant = plantProvider.Plant;

            SeedInvitations(dbContext, plant);
        }

        private static void SeedCurrentUserAsPerson(IPOContext dbContext, ICurrentUserProvider userProvider)
        {
            var personRepository = new PersonRepository(dbContext);
            personRepository.Add(new Person(userProvider.GetCurrentUserOid(), "Siri", "Seed"));
            dbContext.SaveChangesAsync().Wait();
        }

        private static void SeedInvitations(IPOContext dbContext, string plant)
        {
            var invitationRepository = new InvitationRepository(dbContext);
            var invitation = new Invitation(plant, SeedingData.ProjectCode, SeedingData.Invitation, SeedingData.InvitationDescription, DisciplineType.DP);
            invitationRepository.Add(invitation);
            dbContext.SaveChangesAsync().Wait();
        }
    }
}
