using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CompletePunchOut
{
    [TestClass]
    public class CompletePunchOutCommandHandlerTests
    {
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<ILogger<CompletePunchOutCommandHandler>> _loggerMock;
        private Mock<IOptionsMonitor<MeetingOptions>> _meetingOptionsMock;
        private Mock<IEmailService> _emailServiceMock;


        private CompletePunchOutCommand _command;
        private CompletePunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName}", _projectGuid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _firstName = "Ola";
        private const string _lastName = "Nordmann";
        private const DisciplineType _typeDp = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private static Guid _azureOid = new Guid("11111111-1111-2222-3333-333333333333");
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _functionalRoleCode = "FR1";
        private Invitation _invitation;
        private const string _note = "note A";
        private const int _participantId1 = 10;
        private const int _participantId2 = 20;
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateAttendedStatusAndNoteOnParticipantForCommand> _participantsToChange =
            new List<UpdateAttendedStatusAndNoteOnParticipantForCommand>
        {
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _participantId1,
                true,
                _note,
                _participantRowVersion1),
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _participantId2,
                true,
                _note,
                _participantRowVersion2)
        };


        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<CompletePunchOutCommandHandler>>();
            _meetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();
            _meetingOptionsMock.Setup(m => m.CurrentValue).Returns(new MeetingOptions() { PcsBaseUrl = "baseUrl" });
            _emailServiceMock = new Mock<IEmailService>();

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid()).Returns(_azureOidForCurrentUser);

            //create invitation
            _invitation = new Invitation(
                    _plant,
                    _project,
                    _title,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(_plant, _project, "Comm", "Mc", "d", "1|2", Guid.Empty) },
                    null)
            { MeetingId = _meetingId };
            var participant1 = new Participant(
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
            participant1.SetProtectedIdForTesting(_participantId1);
            _invitation.AddParticipant(participant1);
            var participant2 = new Participant(
                _plant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                _firstName,
                _lastName,
                null,
                "ola@test.com",
                _azureOid,
                1);
            participant2.SetProtectedIdForTesting(_participantId2);
            _invitation.AddParticipant(participant2);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            var currentPerson = new Person(_azureOidForCurrentUser, _firstName, _lastName, null, null);
            currentPerson.SetProtectedIdForTesting(_participantId1);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(x => x.GetByOidAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(currentPerson));

            //command
            _command = new CompletePunchOutCommand(
                _invitation.Id,
                _invitationRowVersion,
                _participantRowVersion,
                _participantsToChange);

            _dut = new CompletePunchOutCommandHandler(
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _personRepositoryMock.Object,
                _meetingOptionsMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task CompletePunchOutCommand_ShouldCompletePunchOut()
        {
            Assert.AreEqual(IpoStatus.Planned, _invitation.Status);
            var participant = _invitation.Participants.FirstOrDefault();
            Assert.IsNotNull(participant);
            Assert.IsNull(participant.SignedAtUtc);
            Assert.IsNull(participant.SignedBy);

            await _dut.Handle(_command, default);

            Assert.AreEqual(IpoStatus.Completed, _invitation.Status);
            Assert.IsNotNull(participant.SignedAtUtc);
            Assert.AreEqual(_participantId1, participant.SignedBy);
            Assert.IsNotNull(_invitation.CompletedAtUtc);
            Assert.AreEqual(_participantId1, _invitation.CompletedBy);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Assert
            _emailServiceMock.Verify(
                t => t.SendEmailsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once());

        }

        [TestMethod]
        public async Task HandlingCompleteIpoCommand_ShouldSetAndReturnRowVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.AreEqual(_invitationRowVersion, result.Data);
            Assert.AreEqual(_invitationRowVersion, _invitation.RowVersion.ConvertToString());
            Assert.AreEqual(_participantRowVersion, _invitation.Participants.ToList()[0].RowVersion.ConvertToString());
        }

        [TestMethod]
        public async Task HandlingCompleteIpoCommand_ShouldAddAIpoCompletedPostSaveEvent()
        {
            // Assert
            Assert.AreEqual(0, _invitation.PostSaveDomainEvents.Count);

            // Act
            await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(1, _invitation.PostSaveDomainEvents.Count);
            Assert.AreEqual(typeof(IpoCompletedEvent), _invitation.PostSaveDomainEvents.Last().GetType());
        }
    }
}
