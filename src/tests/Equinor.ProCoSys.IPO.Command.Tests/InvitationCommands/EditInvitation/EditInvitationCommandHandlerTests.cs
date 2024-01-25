using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.EditInvitation
{
    [TestClass]
    public class EditInvitationCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;
        private Mock<IOptionsMonitor<MeetingOptions>> _meetingOptionsMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IPermissionCache> _permissionCacheMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<ILogger<EditInvitationCommandHandler>> _loggerMock;

        private EditInvitationCommand _command;
        private EditInvitationCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _rowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAJ00=";
        private const int _participantId = 20;
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private const int _projectId = 320;
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName} project", _projectGuid);
        private const string _title = "Test title";
        private const string _newTitle = "Test title 2";
        private const string _description = "Test description";
        private const string _newDescription = "Test description 2";
        private const string _firstName = "Ola";
        private const string _lastName = "Nordmann";
        private const DisciplineType _typeDp = DisciplineType.DP;
        private const DisciplineType _typeMdp = DisciplineType.MDP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _dpInvitation;
        private Invitation _mdpInvitation;
        private const int _mdpInvitationId = 50;
        private const int _dpInvitationId = 60;

        private static Guid _azureOid = new Guid("11111111-1111-2222-3333-333333333333");
        private static Guid _newAzureOid = new Guid("11111111-2222-2222-3333-333333333333");
        private const string _functionalRoleCode = "FR1";
        private const string _newFunctionalRoleCode = "NEWFR1";
        private const string _functionalRoleWithMultipleEmailsCode = "FR2";
        private const string _functionalRoleWithMultipleInformationEmailsCode = "FR3";
        private const string _mcPkgNo1 = "MC1";
        private const string _mcPkgNo2 = "MC2";
        private const string _mcPkgNo3 = "MC3";
        private const string _commPkgNo = "Comm1";
        private const string _commPkgNo2 = "Comm2";
        private const string _systemPathWithoutSection = "1|2";
        private const string _systemPathWithoutSection2 = "2|2";
        private const string _systemPathWithSection = "14|1|2";
        private const string _systemPathWithSection2 = "15|1|2";

        private readonly List<ParticipantsForEditCommand> _updatedParticipants = new List<ParticipantsForEditCommand>
        {
            new ParticipantsForEditCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForEditCommand(_participantId, _newFunctionalRoleCode, null, _participantRowVersion),
                0),
            new ParticipantsForEditCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForEditCommand(null, _newAzureOid, true, null),
                null,
                1)
        };

        private readonly List<string> _mcPkgScope = new List<string>
        {
            _mcPkgNo1,
            _mcPkgNo2
        };

        private readonly List<string> _commPkgScope = new List<string>
        {
            _commPkgNo
        };

        [TestInitialize]
        public void Setup()
        {
            _project.SetProtectedIdForTesting(_projectId);
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _permissionCacheMock = new Mock<IPermissionCache>();
            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _loggerMock = new Mock<ILogger<EditInvitationCommandHandler>>();

            _meetingClientMock = new Mock<IFusionMeetingClient>();
            _meetingClientMock
                .Setup(x => x.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()))
                .Returns(Task.FromResult(
                new GeneralMeeting(
                new ApiGeneralMeeting
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

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            //mock comm pkg response from main API
            var commPkgDetails = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, CommStatus = "OK", System = _systemPathWithSection };
            IList<ProCoSysCommPkg> pcsCommPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails };
            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, _commPkgScope))
                .Returns(Task.FromResult(pcsCommPkgDetails));

            //mock mc pkg response from main API
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithoutSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithoutSection };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };
            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            //mock person response from main API
            var personDetails = new ProCoSysPerson
            {
                AzureOid = _azureOid.ToString(),
                FirstName = _firstName,
                LastName = _lastName,
                Email = "ola@test.com",
                UserName = "ON"
            };
            var newPersonDetails = new ProCoSysPerson
            {
                AzureOid = _newAzureOid.ToString(),
                FirstName = "Kari",
                LastName = "Nordman",
                Email = "kari@test.com",
                UserName = "KN"
            };
            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _azureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult(personDetails));
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _newAzureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult(newPersonDetails));

            //mock functional role response from main API
            var frDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var newFrDetails = new ProCoSysFunctionalRole
            {
                Code = _newFunctionalRoleCode,
                Description = "FR description2",
                Email = "fr2@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var frMultipleEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleEmailsCode,
                Description = "FR description",
                Email = "fr3@email.com;fr76@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var frMultipleInformationEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleInformationEmailsCode,
                Description = "FR description",
                Email = "fr4@email.com",
                InformationEmail = "ie@email.com;ie2@email.com",
                Persons = null,
                UsePersonalEmail = false
            };
            IList<ProCoSysFunctionalRole> pcsFrDetails = new List<ProCoSysFunctionalRole> { frDetails };
            IList<ProCoSysFunctionalRole> newPcsFrDetails = new List<ProCoSysFunctionalRole> { newFrDetails };
            IList<ProCoSysFunctionalRole> pcsFrMultipleEmailsDetails = new List<ProCoSysFunctionalRole> { frMultipleEmailsDetails };
            IList<ProCoSysFunctionalRole> pcsFrMultipleInformationEmailsDetails = new List<ProCoSysFunctionalRole> { frMultipleInformationEmailsDetails };
            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(pcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _newFunctionalRoleCode }))
                .Returns(Task.FromResult(newPcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleEmailsCode }))
                .Returns(Task.FromResult(pcsFrMultipleEmailsDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleInformationEmailsCode }))
                .Returns(Task.FromResult(pcsFrMultipleInformationEmailsDetails));

            var mcPkgs = new List<McPkg>
            {
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo1, "d", _systemPathWithSection, Guid.Empty, Guid.Empty),
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo2, "d2", _systemPathWithSection, Guid.Empty, Guid.Empty)
            };
            //create invitation
            _dpInvitation = new Invitation(
                    _plant,
                    _project,
                    _title,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    mcPkgs,
                    null)
            { MeetingId = _meetingId };

            var commPkgs = new List<CommPkg>
            {
                new CommPkg(_plant, _project, _commPkgNo, "d", "ok", _systemPathWithSection, Guid.Empty),
                new CommPkg(_plant, _project, _commPkgNo, "d2", "ok", _systemPathWithSection, Guid.Empty)
            };
            //create invitation
            _mdpInvitation = new Invitation(
                    _plant,
                    _project,
                    _title,
                    _description,
                    _typeMdp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg>(),
                    commPkgs)
            { MeetingId = _meetingId };
            _mdpInvitation.SetProtectedIdForTesting(_mdpInvitationId);

            var participant = new Participant(
                _plant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                0);
            participant.SetProtectedIdForTesting(_participantId);
            _dpInvitation.AddParticipant(participant);
            _dpInvitation.AddParticipant(new Participant(
                _plant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                _firstName,
                _lastName,
                null,
                "ola@test.com",
                _azureOid,
                1));
            _dpInvitation.SetProtectedIdForTesting(_dpInvitationId);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(_dpInvitationId))
                .Returns(Task.FromResult(_dpInvitation));

            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(_mdpInvitationId))
                .Returns(Task.FromResult(_mdpInvitation));

            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock.Setup(x => x.GetProjectOnlyByNameAsync(_projectName)).Returns(Task.FromResult(_project));
            _projectRepositoryMock.Setup(x => x.GetByIdAsync(_projectId)).Returns(Task.FromResult(_project));

            _meetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();
            _meetingOptionsMock.Setup(x => x.CurrentValue)
                .Returns(new MeetingOptions { PcsBaseUrl = _plant });

            //command
            _command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                _updatedParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            _dut = new EditInvitationCommandHandler(
                _invitationRepositoryMock.Object,
                _meetingClientMock.Object,
                _plantProviderMock.Object,
                _unitOfWorkMock.Object,
                _mcPkgApiServiceMock.Object,
                _commPkgApiServiceMock.Object,
                _personApiServiceMock.Object,
                _functionalRoleApiServiceMock.Object,
                _meetingOptionsMock.Object,
                _personRepositoryMock.Object,
                _currentUserProviderMock.Object,
                _permissionCacheMock.Object,
                _projectRepositoryMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task MeetingIsUpdatedTest()
        {
            await _dut.Handle(_command, default);

            _meetingClientMock.Verify(x => x.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldUpdateInvitation()
        {
            Assert.AreEqual(_title, _dpInvitation.Title);
            Assert.AreEqual(_description, _dpInvitation.Description);
            Assert.AreEqual(_typeDp, _dpInvitation.Type);

            await _dut.Handle(_command, default);

            Assert.AreEqual(_newDescription, _dpInvitation.Description);
            Assert.AreEqual(_typeMdp, _dpInvitation.Type);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldUpdateScope()
        {
            Assert.AreEqual(2, _dpInvitation.McPkgs.Count);
            Assert.AreEqual(_mcPkgNo1, _dpInvitation.McPkgs.ToList()[0].McPkgNo);
            Assert.AreEqual(_mcPkgNo2, _dpInvitation.McPkgs.ToList()[1].McPkgNo);
            Assert.AreEqual(0, _dpInvitation.CommPkgs.Count);

            await _dut.Handle(_command, default);

            Assert.AreEqual(0, _dpInvitation.McPkgs.Count);
            Assert.AreEqual(1, _dpInvitation.CommPkgs.Count);
            Assert.AreEqual(_commPkgNo, _dpInvitation.CommPkgs.ToList()[0].CommPkgNo);
        }

        [TestMethod]
        public async Task HandleEditInvitationCommand_ShouldBeAbleToAddMcScopeAcrossSystems()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithoutSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithoutSection2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };
            var mcPkgScope = new List<string>
            {
                _mcPkgNo1,
                _mcPkgNo2
            };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                mcPkgScope,
                null,
                _rowVersion);

            await _dut.Handle(command, default);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleEditInvitationCommand_ShouldBeAbleToAddCommScopeAcrossSystems()
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
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, commPkgScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                _updatedParticipants,
                null,
                commPkgScope,
                _rowVersion);

            await _dut.Handle(command, default);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsAcrossSections()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithSection };
            var mcPkgDetails3 = new ProCoSysMcPkg { CommPkgNo = "CommPkgNo3", Description = "D2", Id = 2, McPkgNo = _mcPkgNo3, System = _systemPathWithSection2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2, mcPkgDetails3 };
            var addedScope = new List<string>
            {
                _mcPkgNo1,
                _mcPkgNo2,
                _mcPkgNo3
            };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, addedScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                addedScope,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Mc pkg scope must be within a section"));
        }


        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsHandedOver()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED"};
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1 };
            var addedScope = new List<string>
            {
                _mcPkgNo1
            };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, addedScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                addedScope,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Mc pkgs with signed RFOC cannot be in scope."));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsOnMDP()
        {
            var command = new EditInvitationCommand(
                _mdpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                _updatedParticipants,
                _mcPkgScope,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("MDP must have comm pkg scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsOnDP()
        {
            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("DP must have mc pkg scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfSettingMdpOnIpoWithMcPkgScope()
        {
            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                _updatedParticipants,
                _mcPkgScope,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("MDP must have comm pkg scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfSettingDpOnIpoWithCommPkgScope()
        {
            var command = new EditInvitationCommand(
                _mdpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("DP must have mc pkg scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfClearingScopeOnDP()
        {
            var command = new EditInvitationCommand(
                _mdpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("DP must have mc pkg scope and mc pkg scope only"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfClearingScopeOnMDP()
        {
            var command = new EditInvitationCommand(
                _mdpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                _updatedParticipants,
                null,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("MDP must have comm pkg scope and comm pkg scope only"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsNotFoundInMain()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _systemPathWithSection };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _systemPathWithSection };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };
            var addedScope = new List<string>
            {
                _mcPkgNo1,
                _mcPkgNo2,
                _mcPkgNo3
            };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, addedScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                addedScope,
                null,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find all mc pkgs in scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsAcrossSections()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _systemPathWithSection2 };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var newScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, newScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                newScope,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Comm pkg scope must be within a section"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsHandedOver()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED" };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _systemPathWithSection, OperationHandoverStatus = "ACCEPTED" };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails1, commPkgDetails2 };
            var newScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, newScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                newScope,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Comm pkgs with signed RFOC cannot be in scope."));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsNotFoundInMain()
        {
            var commPkg = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _systemPathWithSection };
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { commPkg };
            var newScope = new List<string>
            {
                _commPkgNo,
                _commPkgNo2
            };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, newScope))
                .Returns(Task.FromResult(commPkgDetails));

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeDp,
                _updatedParticipants,
                null,
                newScope,
                _rowVersion);

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find all comm pkgs in scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfSigningParticipantDoesNotHaveCorrectPrivileges()
        {
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _newAzureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult<ProCoSysPerson>(null));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Person does not have required privileges to be the"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfFunctionalRoleDoesNotExistOrHaveIPOClassification()
        {
            IList<ProCoSysFunctionalRole> functionalRoles = new List<ProCoSysFunctionalRole>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(
                    _plant, new List<string> { _newFunctionalRoleCode }))
                .Returns(Task.FromResult(functionalRoles));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find functional role with functional role code"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldUpdateParticipants()
        {
            Assert.AreEqual(2, _dpInvitation.Participants.Count);
            Assert.AreEqual(_azureOid, _dpInvitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(_functionalRoleCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);

            await _dut.Handle(_command, default);

            Assert.AreEqual(_newAzureOid, _dpInvitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(_newFunctionalRoleCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }

        // todo add test to assert adding new participants

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldSetAndReturnRowVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.AreEqual(_rowVersion, result.Data);
            Assert.AreEqual(_rowVersion, _dpInvitation.RowVersion.ConvertToString());
            Assert.IsTrue(_dpInvitation.Participants.Any(p => p.RowVersion.ConvertToString() == _participantRowVersion));
        }

        [TestMethod]
        public async Task HandlingUpdateInvitationCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInEmailField()
        {
            // Setup
            var newParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(null, _functionalRoleWithMultipleEmailsCode, null, _participantRowVersion),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(null, _azureOid, true, null),
                    null,
                    1)
            };

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                newParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            await _dut.Handle(command, default);

            // Assert
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _meetingClientMock.Verify(x => x.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()), Times.Once);
            Assert.AreEqual(_functionalRoleWithMultipleEmailsCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }

        [TestMethod]
        public async Task HandlingUpdateInvitationCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInInformationEmailField()
        {
            // Setup
            var newParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(null, _functionalRoleWithMultipleInformationEmailsCode, null, _participantRowVersion),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(null, _azureOid, true, null),
                    null,
                    1)
            };

            var command = new EditInvitationCommand(
                _dpInvitationId,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _typeMdp,
                newParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            await _dut.Handle(command, default);

            // Assert
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _meetingClientMock.Verify(x => x.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()), Times.Once);
            Assert.AreEqual(_functionalRoleWithMultipleInformationEmailsCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }

        [TestMethod]
        public async Task HandlingUpdateInvitationCommand_UpdatingInvitationAsUninvitedAdmin_ShouldCallLogErrorOnce()
        {
            // Setup exception in Edit meeting.
            _meetingClientMock.Setup(c => c.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()))
                .Throws(new MeetingApiException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden), ""));

            IList<string> permissions = new List<string> { "IPO/ADMIN" };
            _permissionCacheMock.Setup(i => i.GetPermissionsForUserAsync(
                    _plant, It.IsAny<Guid>()))
                .Returns(Task.FromResult(permissions));

            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo("Unable to edit outlook meeting for IPO as admin.") == 0;

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateInvitationCommand_UpdatingInvitationAsParticipant_FailingMeetingsApi_ShouldThrow()
        {
            // Setup exception in Edit meeting.
            _meetingClientMock.Setup(c => c.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()))
                .Throws(new MeetingApiException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden), ""));

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _dut.Handle(_command, default));
        }
    }
}
