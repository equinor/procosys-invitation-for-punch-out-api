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

        public static void Seed(this IPOContext dbContext, IServiceProvider serviceProvider, KnownTestData knownTestData)
        {
            var userProvider = serviceProvider.GetRequiredService<CurrentUserProvider>();
            var plantProvider = serviceProvider.GetRequiredService<PlantProvider>();
            userProvider.SetCurrentUserOid(new Guid(_seederOid));
            plantProvider.SetPlant(KnownTestData.Plant);
            
            /* 
             * Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
             * This is the first user that is added to the database and will not get "Created" and "CreatedBy" data.
             */
            SeedCurrentUserAsPerson(dbContext, userProvider);

            var plant = plantProvider.Plant;

            var invitation = SeedInvitation(dbContext, plant);
            knownTestData.InvitationIds.Add(invitation.Id);

            var attachment = SeedAttachment(dbContext, invitation);
            knownTestData.AttachmentIds.Add(attachment.Id);

            var commPkg = SeedCommPkg(dbContext, invitation);
            knownTestData.CommPkgIds.Add(commPkg.Id);

            var mcPkg = SeedMcPkg(dbContext, invitation);
            knownTestData.McPkgIds.Add(mcPkg.Id);
        }

        private static void SeedCurrentUserAsPerson(IPOContext dbContext, ICurrentUserProvider userProvider)
        {
            var personRepository = new PersonRepository(dbContext);
            personRepository.Add(new Person(userProvider.GetCurrentUserOid(), "Siri", "Seed"));
            dbContext.SaveChangesAsync().Wait();
        }

        private static Invitation SeedInvitation(IPOContext dbContext, string plant)
        {
            var invitationRepository = new InvitationRepository(dbContext);
            var invitation = new Invitation(
                plant,
                KnownTestData.ProjectName,
                KnownTestData.InvitationTitle,
                KnownTestData.InvitationDescription,
                DisciplineType.DP)
            {
                MeetingId = KnownTestData.MeetingId
            };
            invitationRepository.Add(invitation);
            dbContext.SaveChangesAsync().Wait();

            return invitation;
        }

        private static Attachment SeedAttachment(IPOContext dbContext, Invitation invitation)
        {
            var attachment = new Attachment(invitation.Plant, "Fil1.txt");
            invitation.AddAttachment(attachment);
            dbContext.SaveChangesAsync().Wait();
            return attachment;
        }

        private static CommPkg SeedCommPkg(IPOContext dbContext, Invitation invitation)
        {
            var commPkg = new CommPkg(invitation.Plant, invitation.ProjectName, KnownTestData.CommPkgNo, "Description", "OK");
            invitation.AddCommPkg(commPkg);
            dbContext.SaveChangesAsync().Wait();
            return commPkg;
        }

        private static McPkg SeedMcPkg(IPOContext dbContext, Invitation invitation)
        {
            var mcPkg = new McPkg(invitation.Plant, invitation.ProjectName, KnownTestData.CommPkgNo, KnownTestData.McPkgNo, "Description");
            invitation.AddMcPkg(mcPkg);
            dbContext.SaveChangesAsync().Wait();
            return mcPkg;
        }
    }
}
