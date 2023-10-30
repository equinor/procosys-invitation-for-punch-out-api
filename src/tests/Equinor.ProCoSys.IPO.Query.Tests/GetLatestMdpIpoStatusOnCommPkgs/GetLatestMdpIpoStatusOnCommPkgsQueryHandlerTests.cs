using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetLatestMdpIpoStatusOnCommPkgs
{
    [TestClass]
    public class GetLatestMdpIpoStatusOnCommPkgsQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private int _mdpInvitationId1;
        private int _mdpInvitationId2;
        private const string _commPkgNo1 = "CommPkgNo";
        private const string _commPkgNo2 = "CommPkgNo2";
        private const string _projectName = "Project1";
        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var project = new Project(TestPlant, _projectName, $"Description of {_projectName}", _project1Guid);
                project.SetProtectedIdForTesting(320);

                var meetingId = new Guid("11111111-2222-2222-2222-333333333333");
                var system = "1|2";
                var commPkg1 = new CommPkg(
                    TestPlant,
                    project,
                    _commPkgNo1,
                    "Description",
                    "OK",
                    system);

                var commPkg2 = new CommPkg(
                    TestPlant,
                    project,
                    _commPkgNo2,
                    "Description",
                    "OK",
                    system);

                var mcPkg = new McPkg(
                    TestPlant,
                    project,
                    _commPkgNo2,
                    "McPkgNo",
                    "Description",
                    system);

                var mdpInvitation = new Invitation(
                    TestPlant,
                    project,
                    "MDP with mc pkgs",
                    "Description1",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> { commPkg1, commPkg2 })
                {
                    MeetingId = meetingId
                };

                var mdpInvitation1 = new Invitation(
                    TestPlant,
                    project,
                    "MDP Title",
                    "Description2",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> {commPkg1})
                {
                    MeetingId = meetingId
                };

                var mdpInvitation2 = new Invitation(
                    TestPlant,
                    project,
                    "MDP Title 2",
                    "Description3",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> {commPkg2})
                {
                    MeetingId = meetingId
                };

                var dpInvitation = new Invitation(
                    TestPlant,
                    project,
                    "DP Title",
                    "Description4",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { mcPkg },
                    null)
                {
                    MeetingId = meetingId
                };

                context.Invitations.Add(mdpInvitation);
                context.SaveChangesAsync().Wait();

                var timeProvider = new ManualTimeProvider(new DateTime(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc));
                TimeService.SetProvider(timeProvider);

                context.Projects.Add(project);
                context.Invitations.Add(mdpInvitation1);
                context.Invitations.Add(mdpInvitation2);
                context.Invitations.Add(dpInvitation);
                context.SaveChangesAsync().Wait();
                _mdpInvitationId1 = mdpInvitation1.Id;
                _mdpInvitationId2 = mdpInvitation2.Id;
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> {_commPkgNo1, _commPkgNo2}, _projectName);
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);
                var result = await dut.Handle(query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturn2Invitations()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { _commPkgNo1, _commPkgNo2 }, _projectName);
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var commPkgsWithMdpIposDtos = result.Data;
                Assert.AreEqual(2, commPkgsWithMdpIposDtos.Count);

                Assert.IsNotNull(commPkgsWithMdpIposDtos.SingleOrDefault(i => i.LatestMdpInvitationId == _mdpInvitationId1));
                Assert.IsNotNull(commPkgsWithMdpIposDtos.SingleOrDefault(i => i.LatestMdpInvitationId == _mdpInvitationId2));
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturn1Invitation()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { _commPkgNo2 }, _projectName);
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var commPkgsWithMdpIposDtos = result.Data;
                Assert.AreEqual(1, commPkgsWithMdpIposDtos.Count);

                Assert.IsNotNull(commPkgsWithMdpIposDtos.SingleOrDefault(i => i.LatestMdpInvitationId == _mdpInvitationId2));
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfNoInvitationsFound()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);

                var result = await dut.Handle(new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { "Unknown" }, _projectName), default);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfCommPkgNo_IsNull()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);

                var result = await dut.Handle(new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { null }, _projectName), default);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfUnknwonProject()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetLatestMdpIpoStatusOnCommPkgsQueryHandler(context);

                var result = await dut.Handle(new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { _commPkgNo2 }, "Unknown"), default);
                Assert.AreEqual(0, result.Data.Count);
            }
        }
    }
}
