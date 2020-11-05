using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
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

        private EditInvitationCommand _command;
        private EditInvitationCommandHandler _dut;
        private readonly string _plant = "PCS$TEST_PLANT";
        private readonly string _rowVersion = "AAAAAAAAABA=";
        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _newTitle = "Test title 2";
        private readonly string _description = "Test description";
        private readonly string _newDescription = "Test description 2";
        private readonly DisciplineType _type = DisciplineType.DP;
        private Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _invitation;
        private int _saveChangesCount;
        private static Guid AzureOid = new Guid("11111111-1111-2222-3333-333333333333");
        private static Guid NewAzureOid = new Guid("11111111-2222-2222-3333-333333333333");
        private const string FrCode = "FR1";
        private const string NewFrCode = "NEWFR1";
        private const string McPkgNo1 = "MC1";
        private const string McPkgNo2 = "MC2";
        private const string CommPkgNo = "Comm1";
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand(FrCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(AzureOid,  "Ola", "Nordman", "ola@test.com", true),
                null,
                1)
        };

        private readonly List<ParticipantsForCommand> _updatedParticipants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand(NewFrCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(NewAzureOid,  "Kari", "Nordman", "kari@test.com", true),
                null,
                1)
        };

        private readonly List<string> _mcPkgScope = new List<string>
        {
            McPkgNo1,
            McPkgNo2
        };

        private readonly List<string> _commPkgScope = new List<string>
        {
            CommPkgNo
        };

        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;
        private ProCoSysCommPkg _commPkgDetails;
        private ProCoSysPerson _personDetails;
        private ProCoSysPerson _newPersonDetails;
        private ProCoSysFunctionalRole _frDetails;
        private ProCoSysFunctionalRole _newFrDetails;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

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
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => _saveChangesCount++);

            _commPkgDetails = new ProCoSysCommPkg { CommPkgNo = CommPkgNo, Description = "D1", Id = 1, CommStatus = "OK", SystemId = 123};
            IList<ProCoSysCommPkg> commPkgDetails = new List<ProCoSysCommPkg> { _commPkgDetails };
            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, _commPkgScope))
                .Returns(Task.FromResult(commPkgDetails));

            _mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = CommPkgNo, Description = "D1", Id = 1, McPkgNo = McPkgNo1 };
            _mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = CommPkgNo, Description = "D2", Id = 2, McPkgNo = McPkgNo2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { _mcPkgDetails1, _mcPkgDetails2 };

            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            _personDetails = new ProCoSysPerson
            {
                AzureOid = AzureOid.ToString(),
                FirstName = "Ola",
                LastName = "Nordman",
                Email = "ola@test.com"
            };

            _newPersonDetails = new ProCoSysPerson
            {
                AzureOid = NewAzureOid.ToString(),
                FirstName = "Kari",
                LastName = "Nordman",
                Email = "kari@test.com"
            };

            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(_plant,
                    AzureOid.ToString(), "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(_personDetails));
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(_plant,
                    NewAzureOid.ToString(), "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(_newPersonDetails));

            _frDetails = new ProCoSysFunctionalRole
            {
                Code = FrCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };

            _newFrDetails = new ProCoSysFunctionalRole
            {
                Code = NewFrCode,
                Description = "FR description2",
                Email = "fr2@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole> { _frDetails };
            IList<ProCoSysFunctionalRole> newFrDetails = new List<ProCoSysFunctionalRole> { _newFrDetails };

            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { FrCode }))
                .Returns(Task.FromResult(frDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { NewFrCode }))
                .Returns(Task.FromResult(newFrDetails));

            _invitation = new Invitation(_plant, _projectName, _title, _description, _type) { MeetingId = _meetingId };
            _invitation.AddMcPkg(new McPkg(_plant, _projectName, CommPkgNo, McPkgNo1, "d"));
            _invitation.AddMcPkg(new McPkg(_plant, _projectName, CommPkgNo, McPkgNo2, "d2"));
            _invitation.AddParticipant(new Participant(_plant, _participants[0].Organization, IpoParticipantType.FunctionalRole, _participants[0].FunctionalRole.Code, null,null,null,null,0));
            _invitation.AddParticipant(new Participant(_plant, _participants[1].Organization, IpoParticipantType.Person, null, _participants[1].Person.FirstName, _participants[1].Person.LastName, _participants[1].Person.Email, _participants[1].Person.AzureOid, 1));

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            _command = new EditInvitationCommand(
                _invitation.Id,
                _newTitle,
                _newDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
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
                _functionalRoleApiServiceMock.Object);
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
            Assert.AreEqual(_title, _invitation.Title);
            Assert.AreEqual(_description, _invitation.Description);
            Assert.AreEqual(_type, _invitation.Type);

            await _dut.Handle(_command, default);

            Assert.AreEqual(_newTitle, _invitation.Title);
            Assert.AreEqual(_newDescription, _invitation.Description);
        }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldUpdateScope()
        {
            Assert.AreEqual(2, _invitation.McPkgs.Count);
            Assert.AreEqual(McPkgNo1, _invitation.McPkgs.ToList()[0].McPkgNo);
            Assert.AreEqual(McPkgNo2, _invitation.McPkgs.ToList()[1].McPkgNo);
            Assert.AreEqual(0, _invitation.CommPkgs.Count);

            await _dut.Handle(_command, default);

            Assert.AreEqual(0, _invitation.McPkgs.Count);
            Assert.AreEqual(1, _invitation.CommPkgs.Count);
            Assert.AreEqual(CommPkgNo, _invitation.CommPkgs.ToList()[0].CommPkgNo);
    }

        [TestMethod]
        public async Task HandlingUpdateIpoCommand_ShouldUpdateParticipants()
        {
            Assert.AreEqual(2, _invitation.Participants.Count);
            Assert.AreEqual(AzureOid, _invitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(FrCode, _invitation.Participants.ToList()[0].FunctionalRoleCode);

            await _dut.Handle(_command, default);

            Assert.AreEqual(NewAzureOid, _invitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(NewFrCode, _invitation.Participants.ToList()[0].FunctionalRoleCode);
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
            Assert.AreEqual(_rowVersion, _invitation.RowVersion.ConvertToString());
        }
    }
}
