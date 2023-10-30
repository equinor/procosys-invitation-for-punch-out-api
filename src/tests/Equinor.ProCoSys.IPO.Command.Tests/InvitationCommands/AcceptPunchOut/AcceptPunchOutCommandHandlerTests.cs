using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AcceptPunchOut
{
    [TestClass]
    public class AcceptPunchOutCommandHandlerTests
    {
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;

        private AcceptPunchOutCommand _command;
        private AcceptPunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _project1uid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Project project = new(_plant, _projectName, $"Description of {_projectName} project", _project1uid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _firstName = "Ola";
        private const string _lastName = "Nordmann";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _functionalRoleCode = "FR1";
        private Invitation _invitation;
        private const string _note = "note A";
        private const int _participantId1 = 10;
        private const int _participantId2 = 20;
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateNoteOnParticipantForCommand> _participantsToChange =
            new List<UpdateNoteOnParticipantForCommand>
        {
            new UpdateNoteOnParticipantForCommand(
                _participantId1,
                _note,
                _participantRowVersion1),
            new UpdateNoteOnParticipantForCommand(
                _participantId2,
                _note,
                _participantRowVersion2)
        };

        [TestInitialize]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid()).Returns(_azureOidForCurrentUser);

            //create invitation
            _invitation = new Invitation(
                    _plant,
                    project,
                    _title,
                    _description,
                    _typeDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(_plant, project, "Comm", "Mc", "d", "1|2")},
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
                "OlaN",
                "ola@test.com",
                _azureOidForCurrentUser,
                1);
            participant2.SetProtectedIdForTesting(_participantId2);
            _invitation.AddParticipant(participant2);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            var currentPerson = new Person(_azureOidForCurrentUser, _firstName, _lastName, null, null); 
            currentPerson.SetProtectedIdForTesting(_participantId2);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(x => x.GetByOidAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(currentPerson));

            _invitation.CompleteIpo(
                participant1,
                participant1.RowVersion.ConvertToString(),
                new Person(new Guid(), null, null, null, null), 
                new DateTime());

            //command
            _command = new AcceptPunchOutCommand(
                _invitation.Id,
                _invitationRowVersion,
                _participantRowVersion,
                _participantsToChange);

            _dut = new AcceptPunchOutCommandHandler(_invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _personRepositoryMock.Object);
        }

        [TestMethod]
        public async Task AcceptPunchOutCommand_ShouldAcceptPunchOut()
        {
            Assert.AreEqual(IpoStatus.Completed, _invitation.Status);
            var participant = _invitation.Participants.Single(p => p.Organization == Organization.ConstructionCompany);
            Assert.IsNotNull(participant);
            Assert.IsNull(participant.SignedAtUtc);
            Assert.IsNull(participant.SignedBy);

            await _dut.Handle(_command, default);

            Assert.AreEqual(IpoStatus.Accepted, _invitation.Status);
            Assert.IsNotNull(_invitation.AcceptedAtUtc);
            Assert.AreEqual(_participantId2, _invitation.AcceptedBy);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingAcceptIpoCommand_ShouldSetAndReturnRowVersion()
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
    }
}
