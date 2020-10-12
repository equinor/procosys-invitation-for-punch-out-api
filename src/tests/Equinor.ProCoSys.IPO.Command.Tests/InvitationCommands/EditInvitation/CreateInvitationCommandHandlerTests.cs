using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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

        private readonly string _plant = "PCS$TEST_PLANT";
        private Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private readonly List<Guid> _requiredParticipantIds = new List<Guid>() { new Guid("22222222-3333-3333-3333-444444444444") };
        private readonly List<string> _requiredParticipantEmails = new List<string>() { "abc@example.com" };
        private readonly List<Guid> _optionalParticipantIds = new List<Guid>() { new Guid("33333333-4444-4444-4444-555555555555") };
        private readonly List<string> _optionalParticipantEmails = new List<string>() { "def@example.com" };
        private Invitation _invitation;

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

            _invitation = new Invitation(_plant) { MeetingId = _meetingId };

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));
        }

        [TestMethod]
        public async Task MeetingIsUpdatedTest()
        {
            var command = new EditInvitationCommand(
                    10,
                    new EditMeetingCommand(
                        "title",
                        "body",
                        "location",
                        new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                        new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                        _requiredParticipantIds,
                        _requiredParticipantEmails,
                        _optionalParticipantIds,
                        _optionalParticipantEmails));

            var dut = new EditInvitationCommandHandler(_invitationRepositoryMock.Object, _meetingClientMock.Object);

            var result = await dut.Handle(command, default);

            _meetingClientMock.Verify(x => x.UpdateMeetingAsync(_meetingId, It.IsAny<Action<GeneralMeetingPatcher>>()), Times.Once);
        }
    }
}
