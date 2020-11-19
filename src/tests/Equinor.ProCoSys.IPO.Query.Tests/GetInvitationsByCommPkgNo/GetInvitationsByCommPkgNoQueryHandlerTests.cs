using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.Test.Common;
using Fusion.Integration.Http.Models;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
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

        private Mock<IFusionMeetingClient> _meetingClientMock;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var MeetingId = new Guid("11111111-2222-2222-2222-333333333333");
                var PersonAzureOid = new Guid("44444444-5555-5555-5555-666666666666");
                const string Description = "Description";

                var FunctionalRoleParticipant = new Participant(
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

                var PersonParticipant = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName",
                    "LastName",
                    "UN",
                    "P1@email.com",
                    PersonAzureOid,
                    1);

                var CommPkg = new CommPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    Description,
                    "OK");

                var McPkg1 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    "McPkgNo1",
                    Description);

                var McPkg2 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    "McPkgNo2",
                    Description);

                _dpInvitation = new Invitation(TestPlant, _projectName, "DP Title", "Description1", DisciplineType.DP)
                {
                    MeetingId = MeetingId
                };

                _dpInvitation.AddParticipant(FunctionalRoleParticipant);
                _dpInvitation.AddParticipant(PersonParticipant);
                _dpInvitation.AddMcPkg(McPkg1);
                _dpInvitation.AddMcPkg(McPkg2);

                _mdpInvitation = new Invitation(TestPlant, _projectName, "MDP Title", "Description2", DisciplineType.MDP)
                {
                    MeetingId = MeetingId
                };

                _mdpInvitation.AddParticipant(FunctionalRoleParticipant);
                _mdpInvitation.AddParticipant(PersonParticipant);
                _mdpInvitation.AddCommPkg(CommPkg);

                _meetingClientMock = new Mock<IFusionMeetingClient>();
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Returns(Task.FromResult(
                        new GeneralMeeting(
                            new ApiGeneralMeeting()
                            {
                                Classification = string.Empty,
                                Contract = null,
                                Convention = string.Empty,
                                DateCreatedUtc = DateTime.MinValue,
                                DateEnd = new ApiDateTimeTimeZoneModel(),
                                DateStart = new ApiDateTimeTimeZoneModel(),
                                ExternalId = null,
                                Id = MeetingId,
                                InviteBodyHtml = string.Empty,
                                IsDisabled = false,
                                IsOnlineMeeting = false,
                                Location = string.Empty,
                                Organizer = new ApiPersonDetailsV1(),
                                OutlookMode = string.Empty,
                                Participants = new List<ApiMeetingParticipant>()
                                {
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = "P1@email.com"
                                        },
                                        OutlookResponse = "Required"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = "FR1@email.com"
                                        },
                                        OutlookResponse = "Accepted"
                                    }
                                },
                                Project = null,
                                ResponsiblePersons = new List<ApiPersonDetailsV1>(),
                                Series = null,
                                Title = string.Empty
                            })));

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
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context, _meetingClientMock.Object);
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
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context, _meetingClientMock.Object);

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
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldThrowException_IfMeetingIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Returns(Task.FromResult<GeneralMeeting>(null));

                var query = new GetInvitationsByCommPkgNoQuery(_commPkgNo, _projectName);
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context, _meetingClientMock.Object);

                await Assert.ThrowsExceptionAsync<Exception>(() => dut.Handle(query, default));
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsByCommPkgNoQuery_ShouldReturnEmptyListOfInvitations_IfNoInvitationsFound()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetInvitationsByCommPkgNoQueryHandler(context, _meetingClientMock.Object);

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
