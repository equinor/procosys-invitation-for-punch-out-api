using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<IOptionsMonitor<MeetingOptions>> _meetingOptionsMock;
        private Mock<IDbContextTransaction> _transactionMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IPersonRepository> _personRepositoryMock;

        private const string _functionalRoleCode = "FR1";
        private const string _mcPkgNo1 = "MC1";
        private const string _mcPkgNo2 = "MC2";
        private const string _commPkgNo = "Comm1";
        private const string _commPkgNo2 = "Comm2";
        private const string _system = "1|2";
        private const string _system2 = "2|2";
        private static Guid _azureOid = new Guid("11111111-1111-2222-2222-333333333333");

        private readonly string _plant = "PCS$TEST_PLANT";
        List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new CreateFunctionalRoleForCommand(_functionalRoleCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new CreatePersonForCommand(_azureOid, "ola@test.com", true),
                null,
                1)
        };

        private ProCoSysPerson _personDetails;
        private ProCoSysFunctionalRole _functionalRoleDetails;

        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _description = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;
        private readonly List<string> _mcPkgScope = new List<string>
        {
            _mcPkgNo1,
            _mcPkgNo2
        };

        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;

        private Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _createdInvitation;
        private CreateInvitationCommandHandler _dut;
        private CreateInvitationCommand _command;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();

            _personRepositoryMock = new Mock<IPersonRepository>();

            _meetingClientMock = new Mock<IFusionMeetingClient>();
            _meetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
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
                    Participants = new List<ApiMeetingParticipant>(),
                    Project = null,
                    ResponsiblePersons = new List<ApiPersonDetailsV1>(),
                    Series = null,
                    Title = string.Empty
                })));

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.Add(It.IsAny<Invitation>()))
                .Callback<Invitation>(x => _createdInvitation = x);

            _transactionMock = new Mock<IDbContextTransaction>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_transactionMock.Object));

            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();

            _mcPkgDetails1 = new ProCoSysMcPkg {CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _system};
            _mcPkgDetails2 = new ProCoSysMcPkg {CommPkgNo = _commPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _system};
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg>{ _mcPkgDetails1, _mcPkgDetails2 };

            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            _personDetails = new ProCoSysPerson
            {
                AzureOid = _azureOid.ToString(),
                FirstName = "Ola",
                LastName = "Nordman",
                Email = "ola@test.com"
            };

            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _azureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult(_personDetails));

            _functionalRoleDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole>{ _functionalRoleDetails };

            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(frDetails));

            _meetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();

            _command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                _mcPkgScope,
                null);

            _dut = new CreateInvitationCommandHandler(
                _plantProviderMock.Object,
                _meetingClientMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _commPkgApiServiceMock.Object,
                _mcPkgApiServiceMock.Object,
                _personApiServiceMock.Object,
                _functionalRoleApiServiceMock.Object,
                _meetingOptionsMock.Object,
                _personRepositoryMock.Object,
                _currentUserProviderMock.Object,
                new Mock<ILogger<CreateInvitationCommandHandler>>().Object);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldAddInvitationToRepository()
        {
            await _dut.Handle(_command, default);

            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldAddMcPkgsToInvitation()
        {
            await _dut.Handle(_command, default);

            var mcPkgs = _createdInvitation.McPkgs.Select(mc => mc).ToList();
            Assert.AreEqual(mcPkgs.Count, 2);
            Assert.AreEqual(mcPkgs[0].McPkgNo, _mcPkgNo1);
            Assert.AreEqual(mcPkgs[1].McPkgNo, _mcPkgNo2);
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfMcScopeIsAcrossSystems()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _system };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _system2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                _mcPkgScope,
                null);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Mc pkg scope must be within a system"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfMcScopeIsNotFoundInMain()
        {
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { _mcPkgDetails1 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                _mcPkgScope,
                null);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find all mc pkgs in scope"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsAcrossSystems()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _system };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _system2 };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                commPkgScope);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Comm pkg scope must be within a system"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsNotFoundInMain()
        {
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { 
                    new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _system }
                };

            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                commPkgScope);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find all comm pkgs in scope"));
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldAddParticipantsToInvitation()
        {
            await _dut.Handle(_command, default);

            var participants = _createdInvitation.Participants.Select(p => p).ToList();
            Assert.AreEqual(participants.Count, 2);
            Assert.AreEqual(participants[0].FunctionalRoleCode, _functionalRoleCode);
            Assert.IsNull(participants[1].FunctionalRoleCode);
            Assert.AreEqual(participants[1].AzureOid, _azureOid);
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfSigningParticipantDoesNotHaveCorrectPrivileges()
        {
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _azureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult<ProCoSysPerson>(null));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Person does not have required privileges to be the"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfFunctionalRoleDoesNotExistOrHaveIPOClassification()
        {
            IList<ProCoSysFunctionalRole> functionalRoles = new List<ProCoSysFunctionalRole>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(
                    _plant, new List<string> {_functionalRoleCode}))
                .Returns(Task.FromResult(functionalRoles));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find functional role with functional role code"));
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldCreateMeetingAndMeetingIdToInvitation()
        {
            await _dut.Handle(_command, default);

            _meetingClientMock.Verify(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()), Times.Once);
            Assert.AreEqual(_meetingId, _createdInvitation.MeetingId);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldRollbackIfFusionApiFails()
        {
            _meetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Throws(new Exception("Something failed"));
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _dut.Handle(_command, default));
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
