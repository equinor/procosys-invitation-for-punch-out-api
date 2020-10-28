using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Fusion.Integration.Http.Models;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationById
{
    [TestClass]
    public class GetInvitationByIdQueryHandlerTests : ReadOnlyTestsBase
    {
        private Invitation _invitation;
        private int _invitationId = 246;
        
        private Mock<IFusionMeetingClient> _meetingClientMock;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var MeetingId = new Guid("11111111-2222-2222-2222-333333333333");
                var PersonAzureOid = new Guid("44444444-5555-5555-5555-666666666666");
                const string ProjectName = "Project1";
                const string Description = "Description";
                const string CommPkgNo = "CommPkgNo";

                var FunctionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    "FR1",
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
                    "P1@email.com",
                    PersonAzureOid,
                    1);

                var CommPkg = new CommPkg(
                    TestPlant,
                    ProjectName,
                    CommPkgNo,
                    Description,
                    "OK");

                var McPkg = new McPkg(
                    TestPlant,
                    ProjectName,
                    CommPkgNo,
                    "McPkgNo",
                    Description);

                _invitation = new Invitation(TestPlant, ProjectName, "Title", "Description", DisciplineType.DP)
                {
                    MeetingId = MeetingId
                };

                _invitation.AddParticipant(FunctionalRoleParticipant);
                _invitation.AddParticipant(PersonParticipant);
                _invitation.AddCommPkg(CommPkg);
                _invitation.AddMcPkg(McPkg);

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
                                Participants = new List<ApiMeetingParticipant>(),
                                Project = null,
                                ResponsiblePersons = new List<ApiPersonDetailsV1>(),
                                Series = null,
                                Title = string.Empty
                            })));


                _invitation.SetProtectedIdForTesting(_invitationId);
                context.Invitations.Add(_invitation);
                context.SaveChangesAsync().Wait();
            }
        }


        [TestMethod]
        public async Task Handler_ShouldReturnNotFound_IfInvitationIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                const int UnknownId = 500;
                var query = new GetInvitationByIdQuery(UnknownId);
                var dut = new GetInvitationByIdQueryHandler(context, _meetingClientMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.NotFound, result.ResultType);
                Assert.IsNull(result.Data);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectInvitation()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationByIdQuery(_invitationId);
                var dut = new GetInvitationByIdQueryHandler(context, _meetingClientMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _invitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldThrowException_IfMeetingIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Returns(Task.FromResult<GeneralMeeting>(null));

                var query = new GetInvitationByIdQuery(_invitationId);
                var dut = new GetInvitationByIdQueryHandler(context, _meetingClientMock.Object);

                await Assert.ThrowsExceptionAsync<Exception>(() => dut.Handle(query, default));
            }
        }

        private static void AssertInvitation(InvitationDto invitationDto, Invitation invitation)
        {
            var functionalRoleParticipant = invitation.Participants.First();
            var personParticipant = invitation.Participants.Last();
            var commPkg = invitation.CommPkgs.First();
            var mcPkg = invitation.McPkgs.First();

            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Description, invitationDto.Description);
            Assert.AreEqual(invitation.ProjectName, invitationDto.ProjectName);
            Assert.AreEqual(invitation.Type, invitationDto.Type);
            Assert.AreEqual(functionalRoleParticipant.FunctionalRoleCode, invitationDto.Participants.First().FunctionalRole.Code);
            Assert.AreEqual(personParticipant.AzureOid.ToString(), invitationDto.Participants.Last().Person.AzureOid);
            Assert.AreEqual(commPkg.CommPkgNo, invitationDto.CommPkgScope.First().CommPkgNo);
            Assert.AreEqual(mcPkg.McPkgNo, invitationDto.McPkgScope.First().McPkgNo);
        }
    }
}
