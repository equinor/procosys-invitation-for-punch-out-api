using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
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

            var project = SeedProject(dbContext);

            var mdpInvitation = SeedMdpInvitation(dbContext, plant, project);
            knownTestData.MdpInvitationIds.Add(mdpInvitation.Id);

            var comment = SeedComment(dbContext, mdpInvitation);
            knownTestData.CommentIds.Add(comment.Id);
            
            SeedContractor(dbContext, mdpInvitation);
            SeedConstructionCompany(dbContext, mdpInvitation);

            var dpInvitation = SeedDpInvitation(dbContext, plant, project);
            knownTestData.DpInvitationIds.Add(dpInvitation.Id);

            SeedContractor(dbContext, dpInvitation);
            SeedConstructionCompany(dbContext, dpInvitation);
        }

        private static Project SeedProject(IPOContext dbContext)
        {
            var project = new Project(KnownTestData.Plant, KnownTestData.ProjectName, $"Description for {KnownTestData.ProjectName}");
            var projectRepository = new ProjectRepository(dbContext);
            projectRepository.Add(project);
            dbContext.SaveChangesAsync().Wait();
            return project;
        }

        private static void SeedCurrentUserAsPerson(IPOContext dbContext, ICurrentUserProvider userProvider)
        {
            var personRepository = new PersonRepository(dbContext);
            personRepository.Add(new Person(userProvider.GetCurrentUserOid(), "Siri", "Seed", "ss", "ss@pcs.pcs"));
            dbContext.SaveChangesAsync().Wait();
        }

        private static Invitation SeedMdpInvitation(IPOContext dbContext, string plant, Project project)
        {

            var commPkg = new CommPkg(plant, project, KnownTestData.CommPkgNo, "Description", "OK",
                "1|2");
            var invitationRepository = new InvitationRepository(dbContext);
            var seedMdpInvitation = new Invitation(
                plant,
                project,
                $"{KnownTestData.InvitationTitle} MDP",
                KnownTestData.InvitationDescription,
                DisciplineType.MDP,
                new DateTime(2020, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc),
                null,
                null,
                new List<CommPkg> {commPkg})
            {
                MeetingId = KnownTestData.MeetingId
            };
            invitationRepository.Add(seedMdpInvitation);
            dbContext.SaveChangesAsync().Wait();

            return seedMdpInvitation;
        }

        private static Invitation SeedDpInvitation(IPOContext dbContext, string plant, Project project)
        {
            var mcPkg = new McPkg(plant, project, KnownTestData.CommPkgNo,
                KnownTestData.McPkgNo, "Description", KnownTestData.System);
            var invitationRepository = new InvitationRepository(dbContext);
            var dpInvitation = new Invitation(
                plant,
                project,
                $"{KnownTestData.InvitationTitle} DP",
                KnownTestData.InvitationDescription,
                DisciplineType.DP,
                new DateTime(2020, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc),
                null,
                new List<McPkg> {mcPkg},
                null)
            {
                MeetingId = KnownTestData.MeetingId
            };
            invitationRepository.Add(dpInvitation);
            dbContext.SaveChangesAsync().Wait();

            return dpInvitation;
        }

        private static Comment SeedComment(IPOContext dbContext, Invitation invitation)
        {
            var comment = new Comment(invitation.Plant, "comment text");
            invitation.AddComment(comment);
            dbContext.SaveChangesAsync().Wait();
            return comment;
        }
        
        private static void SeedContractor(IPOContext dbContext, Invitation invitation)
        {
            var contractor = new Participant(
                invitation.Plant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                KnownTestData.FunctionalRoleCode,
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
