using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.SignPunchOut
{
    [TestClass]
    public class SignPunchOutCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;

        private SignPunchOutCommand _command;
        private SignPunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const DisciplineType _type = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private int _saveChangesCount;
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _functionalRoleCode = "FR1";
        private Invitation _invitation;
        private const int _participantId = 10;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => _saveChangesCount++);

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid()).Returns(_azureOidForCurrentUser);


            //mock person response from main API
            var personDetails = new ProCoSysPerson
            {
                AzureOid = _azureOidForCurrentUser.ToString(),
                FirstName = "Kari",
                LastName = "Nordman",
                Email = "kari@test.com",
                UserName = "KN"
            };

            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonInFunctionalRoleAsync(_plant,
                    _azureOidForCurrentUser.ToString(), _functionalRoleCode))
                .Returns(Task.FromResult(personDetails));

            //create invitation
            _invitation = new Invitation(
                _plant,
                _projectName,
                _title,
                _description,
                _type,
                new DateTime(),
                new DateTime(),
                null) { MeetingId = _meetingId };
            var participant = new Participant(
                _plant,
                Organization.Operation,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                3);
            participant.SetProtectedIdForTesting(_participantId);
            _invitation.AddParticipant(participant);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            var currentUser = new Person(_azureOidForCurrentUser, null, null, null, null);
            currentUser.SetProtectedIdForTesting(_participantId);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(x => x.GetByOidAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(currentUser));

            //command
            _command = new SignPunchOutCommand(
                _invitation.Id,
                _participantId,
                _participantRowVersion);

            _dut = new SignPunchOutCommandHandler(
                _plantProviderMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _personApiServiceMock.Object,
                _personRepositoryMock.Object);
        }

        [TestMethod]
        public async Task SignPunchOutCommand_ShouldSignPunchOut()
        {
            var participant = _invitation.Participants.Single(p => p.Id == _participantId);
            Assert.IsNotNull(participant);
            Assert.IsNull(participant.SignedAtUtc);
            Assert.IsNull(participant.SignedBy);

            await _dut.Handle(_command, default);

            Assert.IsNotNull(participant.SignedAtUtc);
            Assert.AreEqual(_participantId, participant.SignedBy);
            Assert.AreEqual(1, _saveChangesCount);
        }

        [TestMethod]
        public async Task HandlingSignIpoCommand_ShouldSetAndReturnRowVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.AreEqual(_participantRowVersion, result.Data);
            Assert.AreEqual(_participantRowVersion, _invitation.Participants.Single(p => p.Id == _participantId).RowVersion.ConvertToString());
        }
    }
}
