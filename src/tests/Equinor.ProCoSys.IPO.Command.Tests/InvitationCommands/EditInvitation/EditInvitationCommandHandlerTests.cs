using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
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

        private EditInvitationCommand _command;
        private EditInvitationCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _rowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private const int _participantId = 20;
        private const string _projectName = "Project name";
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
        private const string _mcPkgNo1 = "MC1";
        private const string _mcPkgNo2 = "MC2";
        private const string _mcPkgNo3 = "MC3";
        private const string _commPkgNo = "Comm1";
        private const string _commPkgNo2 = "Comm2";
        private const string _system = "1|2";
        private const string _system2 = "2|2";
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand(_functionalRoleCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(_azureOid, "ola@test.com", true),
                null,
                1)
        };

        private readonly List<ParticipantsForCommand> _updatedParticipants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand(_newFunctionalRoleCode, null, _participantId, _participantRowVersion),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(_newAzureOid, "kari@test.com", true),
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
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _personRepositoryMock = new Mock<IPersonRepository>();

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
            var commPkgDetails = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, CommStatus = "OK", System = _system};
            IList<ProCoSysCommPkg> pcsCommPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails };
            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, _commPkgScope))
                .Returns(Task.FromResult(pcsCommPkgDetails));

            //mock mc pkg response from main API
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _system};
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _system};
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
            IList<ProCoSysFunctionalRole> pcsFrDetails = new List<ProCoSysFunctionalRole> { frDetails };
            IList<ProCoSysFunctionalRole> newPcsFrDetails = new List<ProCoSysFunctionalRole> { newFrDetails };
            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(pcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _newFunctionalRoleCode }))
                .Returns(Task.FromResult(newPcsFrDetails));

            var mcPkgs = new List<McPkg>
            {
                new McPkg(_plant, _projectName, _commPkgNo, _mcPkgNo1, "d", _system),
                new McPkg(_plant, _projectName, _commPkgNo, _mcPkgNo2, "d2", _system)
            };
            //create invitation
            _dpInvitation = new Invitation(
                    _plant,
                    _projectName,
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
                new CommPkg(_plant, _projectName, _commPkgNo, "d", "ok", _system),
                new CommPkg(_plant, _projectName, _commPkgNo, "d2", "ok", _system)
            };
            //create invitation
            _mdpInvitation = new Invitation(
                    _plant,
                    _projectName,
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
                _participants[0].Organization,
                IpoParticipantType.FunctionalRole,
                _participants[0].FunctionalRole.Code,
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
                _participants[1].Organization,
                IpoParticipantType.Person,
                null,
                _firstName,
                _lastName,
                null,
                _participants[1].Person.Email,
                _participants[1].Person.AzureOid,
                1));
            _dpInvitation.SetProtectedIdForTesting(_dpInvitationId);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(_dpInvitationId))
                .Returns(Task.FromResult(_dpInvitation));

            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(_mdpInvitationId))
                .Returns(Task.FromResult(_mdpInvitation));

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
                _personRepositoryMock.Object);
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
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsAcrossSystems()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _system };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _system };
            var mcPkgDetails3 = new ProCoSysMcPkg { CommPkgNo = "CommPkgNo3", Description = "D2", Id = 2, McPkgNo = _mcPkgNo3, System = _system2 };
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
            Assert.IsTrue(result.Message.StartsWith("Mc pkg scope must be within a system"));
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
            Assert.IsTrue(result.Message.StartsWith("Invitation must have scope"));
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
            Assert.IsTrue(result.Message.StartsWith("Invitation must have scope"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfMcScopeIsNotFoundInMain()
        {
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1, System = _system };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2, System = _system };
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
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsAcrossSystems()
        {
            var commPkgDetails1 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _system };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = _system2 };
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
            Assert.IsTrue(result.Message.StartsWith("Comm pkg scope must be within a system"));
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldThrowErrorIfCommPkgScopeIsNotFoundInMain()
        {
            var commPkg = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D1", Id = 1, System = _system };
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
    }
}
