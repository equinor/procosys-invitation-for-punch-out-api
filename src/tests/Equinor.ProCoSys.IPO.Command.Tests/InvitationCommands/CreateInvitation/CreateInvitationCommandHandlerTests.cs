using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.ICalendar;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private Mock<ICreateEventHelper> _eventHelper;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private Mock<IMcPkgApiForUserService> _mcPkgApiServiceMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<IOptionsMonitor<MeetingOptions>> _meetingOptionsMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IProjectApiForUsersService> _projectApiServiceMock;
        private Mock<ICalendarService> _calendarServiceMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;

        private const string _functionalRoleCode = "FR1";
        private const string _functionalRoleWithMultipleEmailsCode = "FR2";
        private const string _functionalRoleWithMultipleInformationEmailsCode = "FR3";
        private const string _mcPkgNo1 = "MC1";
        private const string _mcPkgNo2 = "MC2";
        private const string _mcPkgNo3 = "MC3";
        private const string _mcPkgNo4 = "MC4";
        private const string _commPkgNo = "Comm1";
        private const string _commPkgNo2 = "Comm2";
        private const string _systemPathWithoutSection = "1|2";
        private const string _systemPathWithoutSection2 = "2|2";
        private const string _systemPathWithSection = "13|1|2";
        private const string _systemPathWithSection2 = "12|1|2";
        private static Guid _azureOid = new Guid("11111111-1111-2222-2222-333333333333");

        private const string _plant = "PCS$TEST_PLANT";
        private readonly Project _project = new(_plant, _projectName, "Description of Project", _project1Guid);
        private readonly Project _project2 = new(_plant, _proCoSysProjectName, _proCoSysProjectDescription, _project2Guid);

        List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForCreateCommand(_functionalRoleCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForCreateCommand(_azureOid, true),
                null,
                1)
        };

        private ProCoSysPerson _personDetails;
        private ProCoSysFunctionalRole _functionalRoleDetails;
        private ProCoSysFunctionalRole _functionalRoleWithMultipleEmailsDetails;
        private ProCoSysFunctionalRole _functionalRoleWithMultipleInformationEmailsDetails;
        private ProCoSysProject _proCoSysProject;
        private ProCoSysProject _proCoSysProject2;

        private const string _projectName = "Project name";
        private const string _proCoSysProjectName = "ProCoSys Project name";
        private const string _proCoSysProjectDescription = "ProCoSys Project description";
        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Guid _project2Guid = new Guid("11111111-2222-2222-2222-333333333342");
        private readonly string _title = "Test title";
        private readonly string _description = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;
        private readonly List<string> _mcPkgScope = new List<string>
        {
            _mcPkgNo1,
            _mcPkgNo2
        };
        private readonly List<string> _mcPkgScope2 = new List<string>
        {
            _mcPkgNo3,
            _mcPkgNo4
        };

        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;
        private ProCoSysMcPkg _mcPkgDetails3;
        private ProCoSysMcPkg _mcPkgDetails4;

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
            _eventHelper = new Mock<ICreateEventHelper>();

            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();

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

            _projectRepositoryMock = new Mock<IProjectRepository>();
            //We want to return a value for _project2 only after it has been added to the context
            _projectRepositoryMock.Setup(x => x.Add(_project2))
                .Callback<Project>(project =>
                    _projectRepositoryMock.Setup(x => x.GetProjectOnlyByNameAsync(_project2.Name)).Returns(Task.FromResult(project)));
            _projectRepositoryMock.Setup(x => x.GetProjectOnlyByNameAsync(_projectName)).Returns(Task.FromResult(_project));


            _proCoSysProject = new ProCoSysProject { Name = _proCoSysProjectName, Description = _proCoSysProjectDescription, IsClosed = false };
            _proCoSysProject2 = new ProCoSysProject { Name = _projectName, Description = "Whatever", IsClosed = false };

            _projectApiServiceMock = new Mock<IProjectApiForUsersService>();
            _projectApiServiceMock
                .Setup(x => x.TryGetProjectAsync(_plant, _proCoSysProjectName, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_proCoSysProject));
            _projectApiServiceMock
                .Setup(x => x.TryGetProjectAsync(_plant, _projectName, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_proCoSysProject2));

            _calendarServiceMock = new Mock<ICalendarService>();
            _emailServiceMock = new Mock<IEmailService>();

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()));

            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();

            _mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithoutSection };
            _mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithoutSection };
            _mcPkgDetails3 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D3", Id = 3, McPkgNo = _mcPkgNo3, System = _systemPathWithoutSection };
            _mcPkgDetails4 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D4", Id = 4, McPkgNo = _mcPkgNo4, System = _systemPathWithoutSection };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { _mcPkgDetails1, _mcPkgDetails2 };
            IList<ProCoSysMcPkg> mcPkgDetails2 = new List<ProCoSysMcPkg> { _mcPkgDetails3, _mcPkgDetails4 };

            _mcPkgApiServiceMock = new Mock<IMcPkgApiForUserService>();
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mcPkgDetails));
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _proCoSysProjectName, _mcPkgScope2, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mcPkgDetails2));

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
            _functionalRoleWithMultipleEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleEmailsCode,
                Description = "FR description",
                Email = "fr@email.com;fr2@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            _functionalRoleWithMultipleInformationEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleInformationEmailsCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = "ie@email.com;ie2@email.com",
                Persons = null,
                UsePersonalEmail = false
            };

            IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { _functionalRoleDetails };
            IList<ProCoSysFunctionalRole> frMultipleEmailsDetails = new List<ProCoSysFunctionalRole> { _functionalRoleWithMultipleEmailsDetails };
            IList<ProCoSysFunctionalRole> frMultipleInformationDetails = new List<ProCoSysFunctionalRole> { _functionalRoleWithMultipleInformationEmailsDetails };

            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(frDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleEmailsCode }))
                .Returns(Task.FromResult(frMultipleEmailsDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleInformationEmailsCode }))
                .Returns(Task.FromResult(frMultipleInformationDetails));

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
                null,
                false);

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
                _projectRepositoryMock.Object,
                _projectApiServiceMock.Object,
                _calendarServiceMock.Object,
                _emailServiceMock.Object,
                _integrationEventPublisherMock.Object,
                _eventHelper.Object,
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
        public async Task HandleCreateInvitationCommand_ShouldBeAbleToAddMcScopeAcrossSystems()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithoutSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithoutSection2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope, It.IsAny<CancellationToken>()))
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
                null,
                false);

            await _dut.Handle(command, default);

            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldBeAbleToAddCommScopeAcrossSystems()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithoutSection };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _systemPathWithoutSection2 };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                DisciplineType.MDP,
                _participants,
                null,
                commPkgScope,
                false);

            await _dut.Handle(command, default);

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
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfMcScopeIsAcrossSection()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithSection2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope, It.IsAny<CancellationToken>()))
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
                null,
                false);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Mc pkg scope must be within a section"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfMcScopeIsHandedOver()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED" };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithSection, OperationHandoverStatus = "SENT" };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope, It.IsAny<CancellationToken>()))
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
                null,
                false);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Mc pkgs with signed RFOC cannot be in scope."));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfMcScopeIsNotFoundInMain()
        {
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { _mcPkgDetails1 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope, It.IsAny<CancellationToken>()))
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
                null,
                false);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find all mc pkgs in scope"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsAcrossSections()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _systemPathWithSection2 };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope, It.IsAny<CancellationToken>()))
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
                commPkgScope,
                false);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Comm pkg scope must be within a section"));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsHandedOver()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED" };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED" };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope, It.IsAny<CancellationToken>()))
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
                commPkgScope,
                false);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Comm pkgs with signed RFOC cannot be in scope."));
        }

        [TestMethod]
        public async Task HandlingCreateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsNotFoundInMain()
        {
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> {
                    new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection }
                };

            var commPkgScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope, It.IsAny<CancellationToken>()))
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
                commPkgScope,
                false);

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
        public async Task HandleCreateInvitationCommand_ParticipantShouldNotHaveDefaultGuid()
        {
            await _dut.Handle(_command, default);

            var participants = _createdInvitation.Participants.Select(p => p).ToList();

            Assert.AreNotEqual(new Guid("00000000-0000-0000-0000-000000000000"), participants.First().Guid);
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
                    _plant, new List<string> { _functionalRoleCode }))
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
        public async Task HandleCreateInvitationCommand_ShouldRollbackIfFusionApiAndICSFails()
        {
            _meetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Throws(new Exception("Could not send invitation through meeting API"));

            _calendarServiceMock
                .Setup(x => x.CreateMessage(It.IsAny<Invitation>(), It.IsAny<string>(), It.IsAny<Person>(), It.IsAny<string>(), It.IsAny<CreateInvitationCommand>()))
                .Throws(new Exception("Could not send invitation as ics through SMTP"));

            await Assert.ThrowsExceptionAsync<IpoSendMailException>(() =>
                _dut.Handle(_command, default));
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(t => t.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldCreateICSIfFusionApiFails()
        {
            _meetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Throws(new Exception("Could not send invitation through meeting API"));

            await _dut.Handle(_command, default);
            _calendarServiceMock.Verify(t => t.CreateMessage(It.IsAny<Invitation>(), It.IsAny<string>(), It.IsAny<Person>(), It.IsAny<string>(), It.IsAny<CreateInvitationCommand>()));
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }


        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInEmailField()
        {
            // Setup
            var participants = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForCreateCommand(_functionalRoleWithMultipleEmailsCode, null),
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForCreateCommand(_azureOid, true),
                    null,
                    1)
            };

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                _mcPkgScope,
                null,
                false);

            await _dut.Handle(command, default);

            // Assert
            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            var invitationParticipants = _createdInvitation.Participants.Select(p => p).ToList();
            Assert.AreEqual(invitationParticipants.Count, 2);
            Assert.AreEqual(invitationParticipants[0].FunctionalRoleCode, _functionalRoleWithMultipleEmailsCode);
            _unitOfWorkMock.Verify(t => t.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInInformationEmailField()
        {
            // Setup
            var participants = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForCreateCommand(_functionalRoleWithMultipleInformationEmailsCode, null),
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForCreateCommand(_azureOid, true),
                    null,
                    1)
            };

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                _mcPkgScope,
                null,
                false);

            await _dut.Handle(command, default);

            // Assert
            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            var invitationParticipants = _createdInvitation.Participants.Select(p => p).ToList();
            Assert.AreEqual(invitationParticipants.Count, 2);
            Assert.AreEqual(invitationParticipants[0].FunctionalRoleCode, _functionalRoleWithMultipleInformationEmailsCode);
            _unitOfWorkMock.Verify(t => t.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldAddProjectToRepositoryIfItDoesNotExist()
        {
            var projectRepositoryTestDouble = new ProjectRepositoryTestDouble();

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
                projectRepositoryTestDouble,
                _projectApiServiceMock.Object,
                _calendarServiceMock.Object,
                _emailServiceMock.Object,
                _integrationEventPublisherMock.Object,
                _eventHelper.Object,
                new Mock<ILogger<CreateInvitationCommandHandler>>().Object);

            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _proCoSysProjectName,
                _type,
                _participants,
                _mcPkgScope2,
                null,
                false);

            await _dut.Handle(command, default);

            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldNotAddProjectToRepositoryIfItDoesExist()
        {
            var projectRepositoryTestDouble = new ProjectRepositoryTestDouble();
            projectRepositoryTestDouble.Add(_project);

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
                projectRepositoryTestDouble,
                _projectApiServiceMock.Object,
                _calendarServiceMock.Object,
                _emailServiceMock.Object,
                _integrationEventPublisherMock.Object,
                _eventHelper.Object,
                new Mock<ILogger<CreateInvitationCommandHandler>>().Object);

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
                null,
                false);

            await _dut.Handle(command, default);

            Assert.IsNotNull(_createdInvitation);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
