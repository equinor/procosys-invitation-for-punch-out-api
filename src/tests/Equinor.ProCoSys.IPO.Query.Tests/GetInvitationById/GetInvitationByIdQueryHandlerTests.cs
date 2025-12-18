using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Fusion.Integration.Http.Models;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationById
{
    [TestClass]
    public class GetInvitationByIdQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private Invitation _mdpInvitation;
        private Invitation _dpInvitation;
        private int _mdpInvitationId;
        private int _dpInvitationId;
        private const int _projectId = 320;

        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<ILogger<GetInvitationByIdQueryHandler>> _loggerMock;

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
                Project.SetProtectedIdForTesting(_projectId);
                const string description = "Description";
                const string commPkgNo = "CommPkgNo";
                const string mcPkgNo = "McPkgNo";
                const string system = "1|2";

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
                    CurrentUserOid,
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
                    Project,
                    commPkgNo,
                    description,
                    "OK",
                    system,
                    Guid.Empty);

                _mdpInvitation = new Invitation(
                    TestPlant,
                    Project,
                    "Title",
                    "Description",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> { commPkg })
                {
                    MeetingId = _meetingId
                };

                _mdpInvitation.AddParticipant(functionalRoleParticipant);
                _mdpInvitation.AddParticipant(personParticipant);
                _mdpInvitation.AddParticipant(functionalRoleParticipant2);
                _mdpInvitation.AddParticipant(frPerson1);
                _mdpInvitation.AddParticipant(frPerson2);

                var mcPkg = new McPkg(
                    TestPlant,
                    Project,
                    commPkgNo,
                    mcPkgNo,
                    description,
                    system,
                    Guid.Empty,
                    Guid.Empty);

                _dpInvitation = new Invitation(
                    TestPlant,
                    Project,
                    "Title 2",
                    "Description 2",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { mcPkg },
                    null)
                {
                    MeetingId = _meetingId
                };

                var functionalRoleParticipantForDp = new Participant(
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

                var personParticipantForDp = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName",
                    "LastName",
                    "UN",
                    _personEmail1,
                    CurrentUserOid,
                    1);

                _dpInvitation.AddParticipant(functionalRoleParticipantForDp);
                _dpInvitation.AddParticipant(personParticipantForDp);

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
                    Persons = new List<ProCoSysPerson>
                    {
                        new ProCoSysPerson
                        {
                            AzureOid = "11111111-2222-2222-2222-333333333332",
                            Email = _frPersonEmail1,
                            FirstName = null,
                            LastName = null,
                            UserName = null
                        },
                        new ProCoSysPerson
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
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode1 },
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));
                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode2 },
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails2));

                _loggerMock = new Mock<ILogger<GetInvitationByIdQueryHandler>>();
                _permissionCacheMock = new Mock<IPermissionCache>();

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
                                            Id = CurrentUserOid,
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

                context.Projects.Add(Project);
                context.Invitations.Add(_mdpInvitation);
                context.Invitations.Add(_dpInvitation);
                context.SaveChangesAsync().Wait();
                _mdpInvitationId = _mdpInvitation.Id;
                _dpInvitationId = _dpInvitation.Id;
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
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.NotFound, result.ResultType);
                Assert.IsNull(result.Data);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectMdpInvitation()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _mdpInvitation);
                Assert.IsTrue(invitationDto.CanEdit);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectDpInvitation()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationByIdQuery(_dpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _dpInvitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectOutlookResponse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var participants = result.Data.Participants.ToList();
                Assert.AreEqual(OutlookResponse.Declined, participants.First().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.None, participants[1].Person.Response);
                Assert.AreEqual(OutlookResponse.Accepted, participants.Last().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Accepted, participants.Last().FunctionalRole.Persons.First().Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Persons.Last().Response);
                Assert.IsTrue(result.Data.CanEdit);
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
                                Organizer = new ApiPersonDetailsV1 { Id = CurrentUserOid },
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
                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var participants = result.Data.Participants.ToList();
                Assert.AreEqual(OutlookResponse.Accepted, participants.First().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Organizer, participants[1].Person.Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Response);
                Assert.AreEqual(OutlookResponse.Declined, participants.Last().FunctionalRole.Persons.First().Response);
                Assert.AreEqual(OutlookResponse.None, participants.Last().FunctionalRole.Persons.Last().Response);
                Assert.IsTrue(result.Data.CanEdit);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnNullAsOutlookResponses_IfUserIsNotInvitedToMeeting()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            {
                _meetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Throws(new Exception("Something failed"));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var participants = result.Data.Participants.ToList();
                Assert.AreEqual(null, participants.First().FunctionalRole.Response);
                Assert.AreEqual(null, participants[1].Person.Response);
                Assert.AreEqual(null, participants.Last().FunctionalRole.Response);
                Assert.AreEqual(null, participants.Last().FunctionalRole.Persons.First().Response);
                Assert.AreEqual(null, participants.Last().FunctionalRole.Persons.Last().Response);
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

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);
                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(null, result.Data.Participants.First().FunctionalRole.Response);
                Assert.IsFalse(result.Data.CanEdit);
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

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _mdpInvitation);
                Assert.IsFalse(invitationDto.CanEdit);
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
                    Persons = new List<ProCoSysPerson>
                        {
                            new ProCoSysPerson
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
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode1 },
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
                var invitationDto = result.Data;
                AssertInvitation(invitationDto, _mdpInvitation);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnWithCanEditAttendedStatusAndNotesForContractor()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode1,
                    Description = "FR description",
                    Email = "fr@email.com",
                    InformationEmail = null,
                    Persons = new List<ProCoSysPerson>
                    {
                        new ProCoSysPerson
                        {
                            AzureOid = CurrentUserOid.ToString(),
                            Email = "test@email.com",
                            FirstName = "FN",
                            LastName = "LN",
                            UserName = "UN"
                        }
                    },
                    UsePersonalEmail = true
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode1 },
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                Assert.IsTrue(invitationDto.Participants.All(participant => participant.CanEditAttendedStatusAndNote));
                Assert.IsFalse(invitationDto.CanDelete);
                Assert.IsTrue(invitationDto.CanCancel);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnWithCannotEditAttendedStatusAndNotesForContractorWhenCompleted()
        {
            var newCurrentUserOid = new Guid("11111111-2222-2222-2222-333333333339");
            var currentUserProviderMock = new Mock<ICurrentUserProvider>();
            currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(newCurrentUserOid);

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;
                var invitation = context.Invitations.Include(i => i.Participants).Single(inv => inv.Id == _mdpInvitationId);
                invitation.CompleteIpo(
                    invitation.Participants.First(),
                    invitation.Participants.First().RowVersion.ConvertToString(),
                    person,
                    DateTime.Now);
                context.SaveChangesAsync().Wait();
            }

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode1,
                    Description = "FR description",
                    Email = "fr@email.com",
                    InformationEmail = null,
                    Persons = new List<ProCoSysPerson>
                    {
                        new ProCoSysPerson
                        {
                            AzureOid = newCurrentUserOid.ToString(),
                            Email = "test@email.com",
                            FirstName = "FN",
                            LastName = "LN",
                            UserName = "UN"
                        }
                    },
                    UsePersonalEmail = true
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode1 }, 
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    currentUserProviderMock.Object,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                Assert.IsFalse(invitationDto.Participants.All(participant => participant.CanEditAttendedStatusAndNote));
                Assert.IsFalse(invitationDto.CanDelete);
                Assert.IsTrue(invitationDto.CanCancel);
            }
        }


        [TestMethod]
        public async Task Handler_ShouldReturnWithCanEditAttendedStatusAndNotesForSignerWhenNotSigned()
        {
            var newCurrentUserOid = new Guid("11111111-2222-2222-2222-333333333332");
            var currentUserProviderMock = new Mock<ICurrentUserProvider>();
            currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(newCurrentUserOid);

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode2,
                    Description = "FR description",
                    Email = "fr@email.com",
                    InformationEmail = null,
                    Persons = new List<ProCoSysPerson>
                    {
                        new ProCoSysPerson
                        {
                            AzureOid = newCurrentUserOid.ToString(),
                            Email = "test@email.com",
                            FirstName = "FN",
                            LastName = "LN",
                            UserName = "UN"
                        }
                    },
                    UsePersonalEmail = true
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode2 }, 
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    currentUserProviderMock.Object,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                Assert.IsFalse(invitationDto.Participants.First().CanEditAttendedStatusAndNote);
                Assert.IsFalse(invitationDto.Participants.ToList()[1].CanEditAttendedStatusAndNote);
                Assert.IsTrue(invitationDto.Participants.ToList()[2].CanEditAttendedStatusAndNote);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnWithCannotEditAttendedStatusAndNotesForSignerWhenSigned()
        {
            var newCurrentUserOid = new Guid("11111111-2222-2222-2222-333333333332");
            var currentUserProviderMock = new Mock<ICurrentUserProvider>();
            currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(newCurrentUserOid);

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;
                var invitation = context.Invitations.Include(i => i.Participants).Single(inv => inv.Id == _mdpInvitationId);
                invitation.SignIpo(
                    invitation.Participants.ToList()[2],
                    person,
                    invitation.Participants.First().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var functionalRoleDetails = new ProCoSysFunctionalRole
                {
                    Code = _functionalRoleCode2,
                    Description = "FR description",
                    Email = "fr@email.com",
                    InformationEmail = null,
                    Persons = new List<ProCoSysPerson>
                    {
                        new ProCoSysPerson
                        {
                            AzureOid = newCurrentUserOid.ToString(),
                            Email = "test@email.com",
                            FirstName = "FN",
                            LastName = "LN",
                            UserName = "UN"
                        }
                    },
                    UsePersonalEmail = true
                };
                IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { functionalRoleDetails };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByCodeAsync(
                        _plantProvider.Plant,
                        new List<string> { _functionalRoleCode2 }, 
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(frDetails));

                var query = new GetInvitationByIdQuery(_mdpInvitationId);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    currentUserProviderMock.Object,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                Assert.IsTrue(invitationDto.Participants.All(participant => !participant.CanEditAttendedStatusAndNote));
                Assert.IsFalse(invitationDto.CanEdit);
                Assert.IsFalse(invitationDto.CanCancel);
                Assert.IsFalse(invitationDto.CanDelete);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnWithCannotEditAttendedStatusAndNotes_ForAdminCreator()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations.Single(inv => inv.Id == _mdpInvitationId);
                invitation.CancelIpo(new Person(CurrentUserOid, "Ola", "N", "olan", "email"));
                context.SaveChangesAsync().Wait();
            }

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                IList<string> ipoAdminPrivilege = new List<string> { "IPO/ADMIN" };
                _permissionCacheMock
                    .Setup(x => x.GetPermissionsForUserAsync(_plantProvider.Plant, CurrentUserOid, It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(ipoAdminPrivilege));

                var query = new GetInvitationByIdQuery(_mdpInvitation.Id);
                var dut = new GetInvitationByIdQueryHandler(
                    context,
                    _meetingClientMock.Object,
                    _currentUserProvider,
                    _functionalRoleApiServiceMock.Object,
                    _plantProvider,
                    _loggerMock.Object);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);

                var invitationDto = result.Data;
                Assert.IsTrue(invitationDto.Participants.All(participant => !participant.CanEditAttendedStatusAndNote));
                Assert.IsTrue(invitationDto.CanDelete);
                Assert.IsFalse(invitationDto.CanCancel);
            }
        }

        private void AssertInvitation(InvitationDto invitationDto, Invitation invitation)
        {
            var functionalRoleParticipant = invitation.Participants.First();
            var personParticipant = invitation.Participants.ToList()[1];
            var commPkgs = invitation.CommPkgs.Count;
            var mcPkgs = invitation.McPkgs.Count;

            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Description, invitationDto.Description);
            Assert.AreEqual(GetProjectById(invitation.ProjectId).Name, invitationDto.ProjectName);
            Assert.AreEqual(invitation.Type, invitationDto.Type);
            Assert.AreEqual(functionalRoleParticipant.FunctionalRoleCode, invitationDto.Participants.First().FunctionalRole.Code);
            Assert.IsFalse(invitationDto.Participants.First().CanEditAttendedStatusAndNote);
            Assert.IsFalse(invitationDto.Participants.First().IsSigner);
            Assert.IsFalse(invitationDto.Participants.First().IsAttendedTouched);
            Assert.AreEqual(personParticipant.AzureOid, invitationDto.Participants.ToList()[1].Person.AzureOid);
            Assert.IsTrue(invitationDto.Participants.ToList()[1].CanEditAttendedStatusAndNote);
            Assert.IsTrue(invitationDto.Participants.ToList()[1].IsSigner);
            Assert.AreEqual(commPkgs, invitationDto.CommPkgScope.Count());
            Assert.AreEqual(mcPkgs, invitationDto.McPkgScope.Count());
        }
    }
}
