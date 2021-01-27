using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Test.Common;
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
        private int _invitationId;
        
        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;

        private string _functionalRoleCode = "FrCode";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var meetingId = new Guid("11111111-2222-2222-2222-333333333333");
                const string projectName = "Project1";
                const string description = "Description";
                const string commPkgNo = "CommPkgNo";

                var functionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode,
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
                    _currentUserOid,
                    1);

                var commPkg = new CommPkg(
                    TestPlant,
                    projectName,
                    commPkgNo,
                    description,
                    "OK",
                    "1|2");

                var mcPkg = new McPkg(
                    TestPlant,
                    projectName,
                    commPkgNo,
                    "McPkgNo",
                    description);

                _invitation = new Invitation(
                    TestPlant,
                    projectName,
                    "Title", 
                    "Description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null)
                {
                    MeetingId = meetingId
                };

                _invitation.AddParticipant(functionalRoleParticipant);
                _invitation.AddParticipant(personParticipant);
                _invitation.AddCommPkg(commPkg);
                _invitation.AddMcPkg(mcPkg);

                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode,
                    Description = "FR description",
                    Email = "fr@email.com",
                    InformationEmail = null,
                    Persons = null,
                    UsePersonalEmail = false
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, new List<string> { _functionalRoleCode }))
                    .Returns(Task.FromResult(frDetails));

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
                                Id = meetingId,
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

                context.Invitations.Add(_invitation);
                context.SaveChangesAsync().Wait();
                _invitationId = _invitation.Id;
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnNotFound_IfInvitationIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                const int UnknownId = 500;
                var query = new GetInvitationByIdQuery(UnknownId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider);

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
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _invitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnIpo_IfMeetingIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Returns(Task.FromResult<GeneralMeeting>(null));

                var query = new GetInvitationByIdQuery(_invitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider);

                var result = await dut.Handle(query, default);
                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _invitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnIpo_IfUserIsNotInvitedToMeeting()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Throws(new Exception("Something failed"));

                var query = new GetInvitationByIdQuery(_invitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _invitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnIpo_IfPersonsInFunctionalRoleHaveAzureOidNull()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                    _currentUserProvider);
                {
                    var functionalRoleDetails = new ProCoSysFunctionalRole
                    {
                        Code = _functionalRoleCode,
                        Description = "FR description",
                        Email = "fr@email.com",
                        InformationEmail = null,
                        Persons = new List<Person>
                        {
                            new Person
                            {
                                AzureOid = null,
                                Email = "test@email.com",
                                FirstName = "FN",
                                LastName = "LN",
                                UserName = "UN"
                            }
                        },
                        UsePersonalEmail = true
                    };
                    IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> {functionalRoleDetails};

                    _functionalRoleApiServiceMock
                        .Setup(x => x.GetFunctionalRolesByCodeAsync(_plantProvider.Plant,
                            new List<string> {_functionalRoleCode}))
                        .Returns(Task.FromResult(frDetails));

                    var query = new GetInvitationByIdQuery(_invitationId);
                    var dut = new GetInvitationByIdQueryHandler(
                        context,
                        _meetingClientMock.Object,
                        _currentUserProvider,
                        _functionalRoleApiServiceMock.Object,
                        _plantProvider);

                    var result = await dut.Handle(query, default);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(ResultType.Ok, result.ResultType);
                    var invitationDto = result.Data;
                    AssertInvitation(invitationDto, _invitation);
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
            Assert.IsFalse(invitationDto.Participants.First().CanSign);
            Assert.AreEqual(personParticipant.AzureOid, invitationDto.Participants.Last().Person.Person.AzureOid);
            Assert.IsTrue(invitationDto.Participants.Last().CanSign);
            Assert.AreEqual(commPkg.CommPkgNo, invitationDto.CommPkgScope.First().CommPkgNo);
            Assert.AreEqual(mcPkg.McPkgNo, invitationDto.McPkgScope.First().McPkgNo);
        }
    }
}
