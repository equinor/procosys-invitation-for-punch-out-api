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
using InvitationDto = Equinor.ProCoSys.IPO.Query.GetInvitationById.InvitationDto;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationById
{
    [TestClass]
    public class GetInvitationByIdQueryHandlerTests : ReadOnlyTestsBase
    {
        private Invitation _invitation;
        private int _invitationId;
        
        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;

        private string _functionalRoleCode1 = "FrCode1";
        private string _functionalRoleCode2 = "FrCode2";
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private string _frEmail1 = "FR1@email.com";
        private string _personEmail1 = "P1@email.com";
        private string _frPersonEmail1 = "frp1@email.com";
        private string _frPersonEmail2 = "frp2@email.com";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                const string projectName = "Project1";
                const string description = "Description";
                const string commPkgNo = "CommPkgNo";

                var functionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode1,
                    null,
                    null,
                    null,
                    _frEmail1,
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
                    _personEmail1,
                    _currentUserOid,
                    1);

                var functionalRoleParticipant2 = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode2,
                    null,
                    null,
                    null,
                    null,
                    null,
                    2);

                var frPerson1 = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.Person,
                    _functionalRoleCode2,
                    "FirstName2",
                    "LastName2",
                    "UN2",
                    _frPersonEmail1,
                    new Guid("11111111-2222-2222-2222-333333333332"),
                    2);

                var frPerson2 = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.Person,
                    _functionalRoleCode2,
                    "FirstName3",
                    "LastName3",
                    "UN3",
                    _frPersonEmail2,
                    new Guid("11111111-2222-2222-2222-333333333331"),
                    2);

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
                    MeetingId = _meetingId
                };

                _invitation.AddParticipant(functionalRoleParticipant);
                _invitation.AddParticipant(personParticipant);
                _invitation.AddParticipant(functionalRoleParticipant2);
                _invitation.AddParticipant(frPerson1);
                _invitation.AddParticipant(frPerson2);
                _invitation.AddCommPkg(commPkg);
                _invitation.AddMcPkg(mcPkg);

                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode1,
                    Description = "FR description",
                    Email = _frEmail1,
                    InformationEmail = null,
                    Persons = null,
                    UsePersonalEmail = false
                };

                var functionalRoleDetails2 = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode2,
                    Description = "FR description",
                    Email = null,
                    InformationEmail = null,
                    Persons = new List<Person>
                    {
                        new Person
                        {
                            AzureOid = "11111111-2222-2222-2222-333333333332",
                            Email = _frPersonEmail1,
                            FirstName = null,
                            LastName = null,
                            UserName = null
                        },
                        new Person
                        {
                            AzureOid = "11111111-2222-2222-2222-333333333331",
                            Email = _frPersonEmail2,
                            FirstName = null,
                            LastName = null,
                            UserName = null
                        }
                    },
                    UsePersonalEmail = true
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };
                IList<ProCoSysFunctionalRole> frDetails2 = new List<ProCoSysFunctionalRole> { functionalRoleDetails2 };

                _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, new List<string> { _functionalRoleCode1 }))
                    .Returns(Task.FromResult(frDetails));
                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, new List<string> { _functionalRoleCode2 }))
                    .Returns(Task.FromResult(frDetails2));

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
                                Id = _meetingId,
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
                                            Mail = _personEmail1
                                        },
                                        OutlookResponse = "None"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frEmail1
                                        },
                                        OutlookResponse = "Declined"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frPersonEmail1
                                        },
                                        OutlookResponse = "Accepted"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frPersonEmail2
                                        },
                                        OutlookResponse = "Declined"
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
        public async Task Handler_ShouldReturnCorrectOutlookResponse()
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
                var participants = result.Data.Participants.ToList();
                Assert.AreEqual(OutlookResponse.Declined, participants.First().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.None, participants[1].Person.Response);
                Assert.AreEqual(OutlookResponse.Accepted, participants.Last().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Accepted, participants.Last().FunctionalRole.Persons.First().Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Persons.Last().Response);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectOutlookResponse2()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
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
                                Id = _meetingId,
                                InviteBodyHtml = string.Empty,
                                IsDisabled = false,
                                IsOnlineMeeting = false,
                                Location = string.Empty,
                                Organizer = new ApiPersonDetailsV1{Id = _currentUserOid },
                                OutlookMode = string.Empty,
                                Participants = new List<ApiMeetingParticipant>()
                                {
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _personEmail1
                                        },
                                        OutlookResponse = "None"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frEmail1
                                        },
                                        OutlookResponse = "Accepted"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frPersonEmail1
                                        },
                                        OutlookResponse = "Declined"
                                    },
                                    new ApiMeetingParticipant()
                                    {
                                        Id = Guid.NewGuid(),
                                        Person = new ApiPersonDetailsV1()
                                        {
                                            Id = Guid.NewGuid(),
                                            Mail = _frPersonEmail2
                                        },
                                        OutlookResponse = "None"
                                    }
                                },
                                Project = null,
                                ResponsiblePersons = new List<ApiPersonDetailsV1>(),
                                Series = null,
                                Title = string.Empty
                            })));
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
                var participants = result.Data.Participants.ToList();
                Assert.AreEqual(OutlookResponse.Accepted, participants.First().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Organizer, participants[1].Person.Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Persons.First().Response);
                Assert.AreEqual(OutlookResponse.None, participants.Last().FunctionalRole.Persons.Last().Response);
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
                Assert.AreEqual(OutlookResponse.None, result.Data.Participants.First().FunctionalRole.Response);
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
                        Code = _functionalRoleCode1,
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
                            new List<string> {_functionalRoleCode1}))
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
            var personParticipant = invitation.Participants.ToList()[1];
            var commPkg = invitation.CommPkgs.First();
            var mcPkg = invitation.McPkgs.First();

            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Description, invitationDto.Description);
            Assert.AreEqual(invitation.ProjectName, invitationDto.ProjectName);
            Assert.AreEqual(invitation.Type, invitationDto.Type);
            Assert.AreEqual(functionalRoleParticipant.FunctionalRoleCode, invitationDto.Participants.First().FunctionalRole.Code);
            Assert.IsFalse(invitationDto.Participants.First().CanSign);
            Assert.AreEqual(personParticipant.AzureOid, invitationDto.Participants.ToList()[1].Person.Person.AzureOid);
            Assert.IsTrue(invitationDto.Participants.ToList()[1].CanSign);
            Assert.AreEqual(commPkg.CommPkgNo, invitationDto.CommPkgScope.First().CommPkgNo);
            Assert.AreEqual(mcPkg.McPkgNo, invitationDto.McPkgScope.First().McPkgNo);
        }
    }
}
