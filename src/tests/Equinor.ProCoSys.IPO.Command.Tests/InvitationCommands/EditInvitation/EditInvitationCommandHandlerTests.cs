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
        private const string Plant = "PCS$TEST_PLANT";
        private const string RowVersion = "AAAAAAAAABA=";
        private const string ProjectName = "Project name";
        private const string Title = "Test title";
        private const string NewTitle = "Test title 2";
        private const string Description = "Test description";
        private const string NewDescription = "Test description 2";
        private const DisciplineType Type = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
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

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(Plant);

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

            //mock comm pkg response from main API
            var commPkgDetails = new ProCoSysCommPkg { CommPkgNo = CommPkgNo, Description = "D1", Id = 1, CommStatus = "OK", SystemId = 123};
            IList<ProCoSysCommPkg> pcsCommPkgDetails = new List<ProCoSysCommPkg> { commPkgDetails };
            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(Plant, ProjectName, _commPkgScope))
                .Returns(Task.FromResult(pcsCommPkgDetails));

            //mock mc pkg response from main API
            var mcPkgDetails1 = new ProCoSysMcPkg { CommPkgNo = CommPkgNo, Description = "D1", Id = 1, McPkgNo = McPkgNo1 };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = CommPkgNo, Description = "D2", Id = 2, McPkgNo = McPkgNo2 };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> { mcPkgDetails1, mcPkgDetails2 };
            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(Plant, ProjectName, _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            //mock person response from main API
            var personDetails = new ProCoSysPerson
            {
                AzureOid = AzureOid.ToString(),
                FirstName = "Ola",
                LastName = "Nordman",
                Email = "ola@test.com"
            };
            var newPersonDetails = new ProCoSysPerson
            {
                AzureOid = NewAzureOid.ToString(),
                FirstName = "Kari",
                LastName = "Nordman",
                Email = "kari@test.com"
            };
            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(Plant,
                    AzureOid.ToString(), "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(personDetails));
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(Plant,
                    NewAzureOid.ToString(), "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(newPersonDetails));

            //mock functional role response from main API
            var frDetails = new ProCoSysFunctionalRole
            {
                Code = FrCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var newFrDetails = new ProCoSysFunctionalRole
            {
                Code = NewFrCode,
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
                .Setup(x => x.GetFunctionalRolesByCodeAsync(Plant, new List<string> { FrCode }))
                .Returns(Task.FromResult(pcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(Plant, new List<string> { NewFrCode }))
                .Returns(Task.FromResult(newPcsFrDetails));

            //create invitation
            _invitation = new Invitation(Plant, ProjectName, Title, Description, Type) { MeetingId = _meetingId };
            _invitation.AddMcPkg(new McPkg(Plant, ProjectName, CommPkgNo, McPkgNo1, "d"));
            _invitation.AddMcPkg(new McPkg(Plant, ProjectName, CommPkgNo, McPkgNo2, "d2"));
            _invitation.AddParticipant(new Participant(Plant, _participants[0].Organization, IpoParticipantType.FunctionalRole, _participants[0].FunctionalRole.Code, null,null,null,null,0));
            _invitation.AddParticipant(new Participant(Plant, _participants[1].Organization, IpoParticipantType.Person, null, _participants[1].Person.FirstName, _participants[1].Person.LastName, _participants[1].Person.Email, _participants[1].Person.AzureOid, 1));

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            //command
            _command = new EditInvitationCommand(
                _invitation.Id,
                NewTitle,
                NewDescription,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                ProjectName,
                Type,
                _updatedParticipants,
                null,
                _commPkgScope,
                RowVersion);

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
            Assert.AreEqual(Title, _invitation.Title);
            Assert.AreEqual(Description, _invitation.Description);
            Assert.AreEqual(Type, _invitation.Type);

            await _dut.Handle(_command, default);

            Assert.AreEqual(NewTitle, _invitation.Title);
            Assert.AreEqual(NewDescription, _invitation.Description);
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
            Assert.AreEqual(RowVersion, result.Data);
            Assert.AreEqual(RowVersion, _invitation.RowVersion.ConvertToString());
        }
    }
}
