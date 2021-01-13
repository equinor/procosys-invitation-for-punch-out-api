using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    [TestClass]
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IPersonRepository> _personRepositoryMock;

        private UpdateAttendedStatusAndNotesOnParticipantsCommand _command;
        private UpdateAttendedStatusAndNotesOnParticipantsCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _note = "Test note";
        private const DisciplineType _type = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";
        private int _saveChangesCount;
        private static Guid _azureOidForCurrentUser = new Guid("12345678-1234-1234-1234-123456789123");
        private const string _functionalRoleCode = "FR1";
        private const int _contractorParticipantId = 20;
        private const int _constructionCompanyParticipantId = 30;
        private Invitation _invitation;

        private readonly List<UpdateAttendedStatusAndNoteOnParticipantForCommand> _participants = new List<UpdateAttendedStatusAndNoteOnParticipantForCommand>
        {
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _contractorParticipantId,
                true,
                _note,
                _participantRowVersion1),
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _constructionCompanyParticipantId,
                false,
                _note,
                _participantRowVersion2)
        };

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
                FirstName = "Ola",
                LastName = "Nordman",
                Email = "ola@test.com",
                UserName = "ON"
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
            participant1.SetProtectedIdForTesting(_contractorParticipantId);
            _invitation.AddParticipant(participant1);
            var participant2 = new Participant(
                _plant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                "Kari",
                "Nordmann",
                "KN",
                "kari@test.com",
                null,
                1);
            participant2.SetProtectedIdForTesting(_constructionCompanyParticipantId);
            _invitation.AddParticipant(participant2);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            var currentPerson = new Person(_azureOidForCurrentUser, null, null, null, null);
            currentPerson.SetProtectedIdForTesting(_contractorParticipantId);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(x => x.GetByOidAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(currentPerson));

            _invitation.CompleteIpo(participant2, participant2.RowVersion.ConvertToString(), currentPerson, new DateTime());

            //command
            _command = new UpdateAttendedStatusAndNotesOnParticipantsCommand(
                _invitation.Id,
                _participants);

            _dut = new UpdateAttendedStatusAndNotesOnParticipantsCommandHandler(
                _plantProviderMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _personApiServiceMock.Object);
        }

        [TestMethod]
        public async Task ChangeAttendedStatusesCommand_ShouldChangeStatuses()
        {
            Assert.AreEqual(false, _invitation.Participants.First().Attended);
            Assert.IsNull(_invitation.Participants.First().Note);

            await _dut.Handle(_command, default);

            Assert.AreEqual(true, _invitation.Participants.First().Attended);
            Assert.AreEqual(_note, _invitation.Participants.First().Note);
        }

        [TestMethod]
        public async Task HandlingCompleteIpoCommand_ShouldSetVersions()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.IsTrue(_invitation.Participants.ToList().Any(p => p.RowVersion.ConvertToString() == _participantRowVersion2));
        }
    }
}
