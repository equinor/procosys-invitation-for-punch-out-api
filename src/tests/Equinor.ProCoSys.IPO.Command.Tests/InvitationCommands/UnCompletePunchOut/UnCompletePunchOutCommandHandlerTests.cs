using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UnCompletePunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UnCompletePunchOut
{
    [TestClass]
    public class UnCompletePunchOutCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;

        private UnCompletePunchOutCommand _command;
        private UnCompletePunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _firstName = "Ola";
        private const string _lastName = "Nordmann";
        private const DisciplineType _type = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _functionalRoleCode = "FR1";
        private Invitation _invitation;
        private const int _participantId = 20;

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
                new PersonForCommand(_azureOidForCurrentUser, "ola@test.com", true),
                null,
                1)
        };

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid()).Returns(_azureOidForCurrentUser);

            //create invitation
            _invitation = new Invitation(
                    _plant,
                    _projectName,
                    _title,
                    _description,
                    _type,
                    new DateTime(),
                    new DateTime(),
                    null)
                { MeetingId = _meetingId };
            var participant1 = new Participant(
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
            _invitation.AddParticipant(participant1);
            participant1.SetProtectedIdForTesting(_participantId);

            var participant2 = new Participant(
                _plant,
                _participants[1].Organization,
                IpoParticipantType.Person,
                null,
                _firstName,
                _lastName,
                "OlaN",
                _participants[1].Person.Email,
                _participants[1].Person.AzureOid,
                1);
            _invitation.AddParticipant(participant2);
            var currentPerson = new Person(_azureOidForCurrentUser, _firstName, _lastName, null, null);

            _invitation.CompleteIpo(participant1, participant1.RowVersion.ConvertToString(), currentPerson, new DateTime());

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();

            //command
            _command = new UnCompletePunchOutCommand(
                _invitation.Id,
                _invitationRowVersion,
                _participantRowVersion);

            _dut = new UnCompletePunchOutCommandHandler(
                _plantProviderMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _mcPkgApiServiceMock.Object);
        }

        [TestMethod]
        public async Task UnCompletePunchOutCommand_ShouldUnCompletePunchOut()
        {
            Assert.AreEqual(IpoStatus.Completed, _invitation.Status);
            var participant = _invitation.Participants.Single(p => p.Organization == Organization.Contractor);
            Assert.IsNotNull(participant);
            Assert.IsNotNull(participant.SignedAtUtc);
            Assert.IsNotNull(participant.SignedBy);
            Assert.IsNotNull(_invitation.CompletedAtUtc);
            Assert.IsNotNull(_invitation.CompletedBy);

            await _dut.Handle(_command, default);

            Assert.AreEqual(IpoStatus.Planned, _invitation.Status);
            Assert.IsNull(participant.SignedAtUtc);
            Assert.IsNull(participant.SignedBy);
            Assert.IsNull(_invitation.CompletedAtUtc);
            Assert.IsNull(_invitation.CompletedBy);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUnCompleteIpoCommand_ShouldSetAndReturnRowVersion()
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
        public async Task HandlingUnCompleteIpoCommand_ShouldNotUnCompleteIfClearingM01DateInMainFails()
        {
            _mcPkgApiServiceMock
                .Setup(x => x.ClearM01DatesAsync(_plant, _projectName, new List<string>(), new List<string>(), null))
                .Throws(new Exception("Something failed"));

            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _dut.Handle(_command, default));
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
