using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsByCommPkgNo
{
    [TestClass]
    public class GetInvitationsByCommPkgNoQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private Invitation _dpInvitation;
        private Invitation _mdpInvitation;
        private int _dpInvitationId;
        private int _mdpInvitationId;
        private const string _commPkgNo = "CommPkgNo";
        private const string _projectName = "Project1";
        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");
        private const int _projectId = 320;
        private const string _system = "1|2";
        private readonly Project _project = new(TestPlant, _projectName, $"Description of {_projectName}", _project1Guid);

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _project.SetProtectedIdForTesting(_projectId);
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

                var functionalRoleParticipant2 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    "FR2",
                    null,
                    null,
                    null,
                    "FR2@email.com",
                    null,
                    0);

                var personParticipant2 = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName2",
                    "LastName",
                    "UN",
                    "P2@email.com",
                    personAzureOid,
                    1);

                var commPkg = new CommPkg(
                    TestPlant,
                    _project,
                    _commPkgNo,
                    description,
                    "OK",
                    "1|2",
                    Guid.Empty);

                var mcPkg1 = new McPkg(
                    TestPlant,
                    _project,
                    _commPkgNo,
                    "McPkgNo1",
                    description,
                    _system,
                    Guid.Empty,
                    Guid.Empty);

                var mcPkg2 = new McPkg(
                    TestPlant,
                    _project,
                    _commPkgNo,
                    "McPkgNo2",
                    description,
                    _system,
                    Guid.Empty,
                    Guid.Empty);

                _dpInvitation = new Invitation(
                    TestPlant,
                    _project,
                    "DP Title",
                    "Description1",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { mcPkg1, mcPkg2 },
                    null)
                {
                    MeetingId = meetingId
                };

                _dpInvitation.AddParticipant(functionalRoleParticipant);
                _dpInvitation.AddParticipant(personParticipant);

                _mdpInvitation = new Invitation(
                    TestPlant,
                    _project,
                    "MDP Title",
                    "Description2",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> { commPkg })
                {
                    MeetingId = meetingId
                };

                _mdpInvitation.AddParticipant(functionalRoleParticipant2);
                _mdpInvitation.AddParticipant(personParticipant2);

                context.Projects.Add(_project);
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
