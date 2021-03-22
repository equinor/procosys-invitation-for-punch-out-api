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

            var seedMdpInvitation = SeedMdpInvitation(dbContext, plant);
            knownTestData.MdpInvitationIds.Add(seedMdpInvitation.Id);

            var commPkg = SeedCommPkg(dbContext, seedMdpInvitation);
            knownTestData.CommPkgIds.Add(commPkg.Id);

            var attachment = SeedAttachment(dbContext, seedMdpInvitation);
            knownTestData.AttachmentIds.Add(attachment.Id);

            var comment = SeedComment(dbContext, seedMdpInvitation);
            knownTestData.CommentIds.Add(comment.Id);
            
            SeedContractor(dbContext, seedMdpInvitation);
            SeedConstructionCompany(dbContext, seedMdpInvitation);

            var seedDpInvitation = SeedDpInvitation(dbContext, plant);
            knownTestData.DpInvitationIds.Add(seedDpInvitation.Id);

            var mcPkg = SeedMcPkg(dbContext, seedDpInvitation);
            knownTestData.McPkgIds.Add(mcPkg.Id);

            SeedContractor(dbContext, seedDpInvitation);
            SeedConstructionCompany(dbContext, seedDpInvitation);
        }

        private static void SeedCurrentUserAsPerson(IPOContext dbContext, ICurrentUserProvider userProvider)
        {
            var personRepository = new PersonRepository(dbContext);
            personRepository.Add(new Person(userProvider.GetCurrentUserOid(), "Siri", "Seed", "ss", "ss@pcs.pcs"));
            dbContext.SaveChangesAsync().Wait();
        }

        private static Invitation SeedMdpInvitation(IPOContext dbContext, string plant)
        {
            var invitationRepository = new InvitationRepository(dbContext);
            var seedMdpInvitation = new Invitation(
                plant,
                KnownTestData.ProjectName,
                $"{KnownTestData.InvitationTitle} MDP",
                KnownTestData.InvitationDescription,
                DisciplineType.MDP,
                new DateTime(2020, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc),
                null)
            {
                MeetingId = KnownTestData.MeetingId
            };
            invitationRepository.Add(seedMdpInvitation);
            dbContext.SaveChangesAsync().Wait();

            return seedMdpInvitation;
        }

        private static Invitation SeedDpInvitation(IPOContext dbContext, string plant)
        {
            var invitationRepository = new InvitationRepository(dbContext);
            var dpInvitation = new Invitation(
                plant,
                KnownTestData.ProjectName,
                $"{KnownTestData.InvitationTitle} DP",
                KnownTestData.InvitationDescription,
                DisciplineType.DP,
                new DateTime(2020, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc),
                null)
            {
                MeetingId = KnownTestData.MeetingId
            };
            invitationRepository.Add(dpInvitation);
            dbContext.SaveChangesAsync().Wait();

            return dpInvitation;
        }

        private static Attachment SeedAttachment(IPOContext dbContext, Invitation invitation)
        {
            var attachment = new Attachment(invitation.Plant, "Fil1.txt");
            invitation.AddAttachment(attachment);
            dbContext.SaveChangesAsync().Wait();
            return attachment;
        }

        private static Comment SeedComment(IPOContext dbContext, Invitation invitation)
        {
            var comment = new Comment(invitation.Plant, "comment text");
            invitation.AddComment(comment);
            dbContext.SaveChangesAsync().Wait();
            return comment;
        }

        private static CommPkg SeedCommPkg(IPOContext dbContext, Invitation invitation)
        {
            var commPkg = new CommPkg(invitation.Plant, invitation.ProjectName, KnownTestData.CommPkgNo, "Description",
                "OK", "1|2");
            invitation.AddCommPkg(commPkg);
            dbContext.SaveChangesAsync().Wait();
            return commPkg;
        }

        private static McPkg SeedMcPkg(IPOContext dbContext, Invitation invitation)
        {
            var mcPkg = new McPkg(invitation.Plant, invitation.ProjectName, KnownTestData.CommPkgNo,
                KnownTestData.McPkgNo, "Description", KnownTestData.System);
            invitation.AddMcPkg(mcPkg);
            dbContext.SaveChangesAsync().Wait();
            return mcPkg;
        }

        private static void SeedContractor(IPOContext dbContext, Invitation invitation)
        {
            var contractor = new Participant(
                invitation.Plant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                "FRC",
                null,
                null,
                null,
                "fr@test.com",
                null,
                0);
            invitation.AddParticipant(contractor);
            dbContext.SaveChangesAsync().Wait();
        }

        private static void SeedConstructionCompany(IPOContext dbContext, Invitation invitation)
        {
            var constructionCompany = new Participant(
                invitation.Plant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                "First",
                "Last",
                "UN",
                "un@test.com",
                new Guid("11111111-1111-2222-3333-333333333333"),
                1);
            invitation.AddParticipant(constructionCompany);
            dbContext.SaveChangesAsync().Wait();
        }
    }
}
