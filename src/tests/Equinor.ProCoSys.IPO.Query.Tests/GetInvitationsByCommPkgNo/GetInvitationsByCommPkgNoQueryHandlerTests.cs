using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsByCommPkgNo
{
    [TestClass]
    public class GetInvitationsByCommPkgNoQueryHandlerTests : ReadOnlyTestsBase
    {
        private Invitation _dpInvitation;
        private Invitation _mdpInvitation;
        private int _dpInvitationId;
        private int _mdpInvitationId;
        private const string _commPkgNo = "CommPkgNo";
        private const string _projectName = "Project1";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var meetingId = new Guid("11111111-2222-2222-2222-333333333333");
                var personAzureOid = new Guid("44444444-5555-5555-5555-666666666666");
                const string description = "Description";

                var functionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    "FR1",
                    null,
                    null,
                    null,
                    "FR1@email.com",
                    null,
                    0);

                var personParticipant = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName",
                    "LastName",
                    "UN",
                    "P1@email.com",
                    personAzureOid,
                    1);

                var commPkg = new CommPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    description,
                    "OK");

                var mcPkg1 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    "McPkgNo1",
                    description);

                var mcPkg2 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    "McPkgNo2",
                    description);

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

                _dpInvitation.AddParticipant(functionalRoleParticipant);
                _dpInvitation.AddParticipant(personParticipant);
                _dpInvitation.AddMcPkg(mcPkg1);
                _dpInvitation.AddMcPkg(mcPkg2);

                _mdpInvitation = new Invitation(
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

                _mdpInvitation.AddParticipant(functionalRoleParticipant);
                _mdpInvitation.AddParticipant(personParticipant);
                _mdpInvitation.AddCommPkg(commPkg);

                context.Invitations.Add(_dpInvitation);
                context.Invitations.Add(_mdpInvitation);
                context.SaveChangesAsync().Wait();
                _dpInvitationId = _dpInvitation.Id;
                _mdpInvitationId = _mdpInvitation.Id;
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsByCommPkgNoQuery(_commPkgNo, _projectName);
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context);
                var result = await dut.Handle(query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnCorrectInvitations()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsByCommPkgNoQuery(_commPkgNo, _projectName);
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDtos = result.Data;
                Assert.AreEqual(2, invitationDtos.Count);

                AssertInvitation(invitationDtos.Single(i => i.Id == _dpInvitationId), _dpInvitation);
                AssertInvitation(invitationDtos.Single(i => i.Id == _mdpInvitationId), _mdpInvitation);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfNoInvitationsFound()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context);

                var result = await dut.Handle(new GetInvitationsByCommPkgNoQuery("Unknown", _projectName), default);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private static void AssertInvitation(InvitationForMainDto invitationDto, Invitation invitation)
        {
            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Description, invitationDto.Description);
            Assert.AreEqual(invitation.Type, invitationDto.Type);
            Assert.AreEqual(invitation.Status, invitationDto.Status);
        }
    }
}
