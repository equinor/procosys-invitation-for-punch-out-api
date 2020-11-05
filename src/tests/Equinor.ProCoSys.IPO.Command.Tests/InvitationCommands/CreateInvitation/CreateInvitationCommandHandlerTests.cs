using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
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

        private const string FrCode = "FR1";
        private const string McPkgNo1 = "MC1";
        private const string McPkgNo2 = "MC2";
        private const string CommPkgNo = "Comm1";
        private static Guid AzureOid = new Guid("11111111-1111-2222-2222-333333333333");

        private readonly string _plant = "PCS$TEST_PLANT";
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

        private ProCoSysPerson _personDetails;
        private ProCoSysFunctionalRole _frDetails;

        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _description = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;
        private readonly List<string> _mcPkgScope = new List<string>
        {
            McPkgNo1,
            McPkgNo2
        };

        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;

        private Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _createdInvitation;
        private int _saveChangesCount;
        private CreateInvitationCommandHandler _dut;
        private CreateInvitationCommand _command;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

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

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => _saveChangesCount++);

            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();

            _mcPkgDetails1 = new ProCoSysMcPkg {CommPkgNo = CommPkgNo, Description = "D1", Id = 1, McPkgNo = McPkgNo1};
            _mcPkgDetails2 = new ProCoSysMcPkg {CommPkgNo = CommPkgNo, Description = "D2", Id = 2, McPkgNo = McPkgNo2};
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg>{ _mcPkgDetails1, _mcPkgDetails2 };

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

            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(_plant,
                    AzureOid.ToString(), "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(_personDetails));

            _frDetails = new ProCoSysFunctionalRole
            {
                Code = FrCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole>{ _frDetails };

            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { FrCode }))
                .Returns(Task.FromResult(frDetails));

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
                _functionalRoleApiServiceMock.Object);
        }

        [TestMethod]
        public async Task Invitation_is_added_to_repository_test()
        {
            await _dut.Handle(_command, default);

            Assert.IsNotNull(_createdInvitation);
            Assert.AreEqual(1, _saveChangesCount);
        }

        [TestMethod]
        public async Task McPkgs_are_added_to_invitation()
        {
            await _dut.Handle(_command, default);

            var mcPkgs = _createdInvitation.McPkgs.Select(mc => mc).ToList();
            Assert.AreEqual(mcPkgs.Count, 2);
            Assert.AreEqual(mcPkgs[0].McPkgNo, McPkgNo1);
            Assert.AreEqual(mcPkgs[1].McPkgNo, McPkgNo2);
        }

        [TestMethod]
        public async Task Participants_are_added_to_invitation()
        {
            await _dut.Handle(_command, default);

            var participants = _createdInvitation.Participants.Select(p => p).ToList();
            Assert.AreEqual(participants.Count, 2);
            Assert.AreEqual(participants[0].FunctionalRoleCode, FrCode);
            Assert.IsNull(participants[1].FunctionalRoleCode);
            Assert.AreEqual(participants[1].AzureOid, AzureOid);
      }

        [TestMethod]
        public async Task Meeting_is_created_and_meeting_id_is_set_on_invitation_test()
        {
            await _dut.Handle(_command, default);

            _meetingClientMock.Verify(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()), Times.Once);
            Assert.AreEqual(_meetingId, _createdInvitation.MeetingId);
        }
    }
}
