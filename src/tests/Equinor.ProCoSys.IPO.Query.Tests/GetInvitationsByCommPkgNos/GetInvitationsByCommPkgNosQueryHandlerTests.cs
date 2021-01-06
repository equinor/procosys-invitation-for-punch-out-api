using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNos;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsByCommPkgNos
{
    [TestClass]
    public class GetInvitationsByCommPkgNosQueryHandlerTests : ReadOnlyTestsBase
    {
        private Invitation _dpInvitation;
        private Invitation _mdpInvitation1;
        private Invitation _mdpInvitation2;
        private int _dpInvitationId;
        private int _mdpInvitationId1;
        private int _mdpInvitationId2;
        private const string _commPkgNo1 = "CommPkgNo";
        private const string _commPkgNo2 = "CommPkgNo2";
        private const string _projectName = "Project1";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var meetingId = new Guid("11111111-2222-2222-2222-333333333333");

                var commPkg1 = new CommPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo1,
                    "Description",
                    "OK");

                var commPkg2 = new CommPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo2,
                    "Description",
                    "OK");

                var mcPkg1 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo1,
                    "McPkgNo1",
                    "Description");

                var mcPkg2 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo1,
                    "McPkgNo2",
                    "Description");

                _dpInvitation = new Invitation(
                    TestPlant,
                    _projectName,
                    "DP Title",
                    "Description1",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null)
                {
                    MeetingId = meetingId
                };

                _dpInvitation.AddMcPkg(mcPkg1);
                _dpInvitation.AddMcPkg(mcPkg2);

                _mdpInvitation1 = new Invitation(
                    TestPlant,
                    _projectName,
                    "MDP Title",
                    "Description2",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null)
                {
                    MeetingId = meetingId
                };

                _mdpInvitation2 = new Invitation(
                    TestPlant,
                    _projectName,
                    "MDP Title 2",
                    "Description3",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null)
                {
                    MeetingId = meetingId
                };

                _mdpInvitation1.AddCommPkg(commPkg1);

                _mdpInvitation2.AddCommPkg(commPkg2);

                context.Invitations.Add(_dpInvitation);
                context.Invitations.Add(_mdpInvitation1);
                context.Invitations.Add(_mdpInvitation2);
                context.SaveChangesAsync().Wait();
                _dpInvitationId = _dpInvitation.Id;
                _mdpInvitationId1 = _mdpInvitation1.Id;
                _mdpInvitationId2 = _mdpInvitation2.Id;
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsByCommPkgNosQuery(new List<string> {_commPkgNo1, _commPkgNo2}, _projectName);
                var dut = new GetInvitationsByCommPkgNosQueryHandler(context);
                var result = await dut.Handle(query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturn3Invitations()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsByCommPkgNosQuery(new List<string> { _commPkgNo1, _commPkgNo2 }, _projectName);
                var dut = new GetInvitationsByCommPkgNosQueryHandler(context);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDtos = result.Data;
                Assert.AreEqual(3, invitationDtos.Count);

                AssertInvitation(invitationDtos.Single(i => i.Id == _dpInvitationId), _dpInvitation);
                AssertInvitation(invitationDtos.Single(i => i.Id == _mdpInvitationId1), _mdpInvitation1);
                AssertInvitation(invitationDtos.Single(i => i.Id == _mdpInvitationId2), _mdpInvitation2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturn2Invitations()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsByCommPkgNosQuery(new List<string> { _commPkgNo1 }, _projectName);
                var dut = new GetInvitationsByCommPkgNosQueryHandler(context);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDtos = result.Data;
                Assert.AreEqual(2, invitationDtos.Count);

                AssertInvitation(invitationDtos.Single(i => i.Id == _dpInvitationId), _dpInvitation);
                AssertInvitation(invitationDtos.Single(i => i.Id == _mdpInvitationId1), _mdpInvitation1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfNoInvitationsFound()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetInvitationsByCommPkgNosQueryHandler(context);

                var result = await dut.Handle(new GetInvitationsByCommPkgNosQuery(new List<string>{"Unknown"}, _projectName), default);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private static void AssertInvitation(InvitationForMainDto invitationDto, Invitation invitation)
        {
            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Type, invitationDto.Type);
            Assert.AreEqual(invitation.Status, invitationDto.Status);
        }
    }
}
