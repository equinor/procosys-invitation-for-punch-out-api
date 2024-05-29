using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.IPO.MessageContracts;
using Fusion.Integration.Meeting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CancelPunchOut
{
    [TestClass]
    public class CancelPunchOutCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IEventRepository> _eventRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IFusionMeetingClient> _fusionMeetingClient;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;
        private Mock<ILogger<CancelPunchOutCommandHandler>> _loggerMock;

        private CancelPunchOutCommand _command;
        private CancelPunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName} project", _projectGuid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private Invitation _invitation;
        private BusEventMessage _busEventMessage;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();
            _eventRepositoryMock = new Mock<IEventRepository>();

            _integrationEventPublisherMock
                .Setup(eventPublisher => eventPublisher.PublishAsync(It.IsAny<BusEventMessage>(), It.IsAny<CancellationToken>()))
                .Callback<BusEventMessage, CancellationToken>((busEventMessage, cancellationToken) =>
                {
                    _busEventMessage = busEventMessage;
                });

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid()).Returns(_azureOidForCurrentUser);

            _loggerMock = new Mock<ILogger<CancelPunchOutCommandHandler>>();

            var currentPerson = new Person(_azureOidForCurrentUser, null, null, null, null);
            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(x => x.GetByOidAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(currentPerson));

            _fusionMeetingClient = new Mock<IFusionMeetingClient>();
            //create invitation
            _invitation = new Invitation(
                    _plant,
                    _project,
                    _title,
                    _description,
                    _typeDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(_plant, _project, "Comm", "Mc", "d", "1|2", Guid.Empty, Guid.Empty)},
                    null)
                { MeetingId = _meetingId };

            var participant = new Participant(_plant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                "FR",
                null,
                null,
                null,
                null,
                null,
                0);

            _invitation.CompleteIpo(participant, "AAAAAAAAABB=", currentPerson, new DateTime());

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));
            _eventRepositoryMock.Setup(x => x.GetInvitationEvent(It.IsAny<Guid>())).Returns(new InvitationEvent());

            //command
            _command = new CancelPunchOutCommand(_invitation.Id, _invitationRowVersion);

            _dut = new CancelPunchOutCommandHandler(
                _invitationRepositoryMock.Object,
                _personRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _fusionMeetingClient.Object,
                _currentUserProviderMock.Object,
                _integrationEventPublisherMock.Object,
                _loggerMock.Object
                );
        }

        [TestMethod]
        public async Task CancelPunchOutCommand_ShouldCancelPunchOut()
        {
            Assert.AreEqual(IpoStatus.Completed, _invitation.Status);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(IpoStatus.Canceled, _invitation.Status);
            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId), Times.Once);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingCancelIpoCommand_ShouldSetAndReturnRowVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.AreEqual(_invitationRowVersion, result.Data);
            Assert.AreEqual(_invitationRowVersion, _invitation.RowVersion.ConvertToString());
            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task HandlingCancelIpoCommand_ShouldAddAIpoCanceledPostSaveEvent()
        {
            // Assert
            Assert.AreEqual(1, _invitation.PostSaveDomainEvents.Count);

            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(2, _invitation.PostSaveDomainEvents.Count);
            Assert.AreEqual(typeof(IpoCanceledEvent), _invitation.PostSaveDomainEvents.Last().GetType());
            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task HandlingCancelIpoCommandMeetingApiException_ShouldCallLogErrorOnce()
        {
            // Setup exception in Delete meeting.
            _fusionMeetingClient.Setup(c => c.DeleteMeetingAsync(_meetingId))
                .Throws(new MeetingApiException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden), ""));
                        
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo("Unable to cancel outlook meeting for IPO.") == 0;

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldSendIpoMessageToServiceBus()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            _integrationEventPublisherMock.Verify(t => t.PublishAsync(It.IsAny<BusEventMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual("Canceled", _busEventMessage.Event);
            Assert.AreEqual(IpoStatus.Canceled, _busEventMessage.IpoStatus);
            Assert.AreEqual(_plant, _busEventMessage.Plant);
            Assert.AreNotEqual(Guid.Empty, _busEventMessage.InvitationGuid);
            Assert.AreNotEqual(Guid.Empty, _busEventMessage.Guid);
        }

        [TestMethod]
        public async Task Handle_ShouldSendInvitationMessage()
        {
            //Arrange
            var invitationEvent = new InvitationEvent { Description = "A Invitation message description" };
            _eventRepositoryMock
                .Setup(x => x.GetInvitationEvent(It.IsAny<Guid>()))
                .Returns(invitationEvent);

            IInvitationEventV1 invitationEventMessage = new InvitationEvent();

            _integrationEventPublisherMock
            .Setup(eventPublisher => eventPublisher.PublishAsync(It.IsAny<IInvitationEventV1>(), It.IsAny<CancellationToken>()))
                .Callback<IInvitationEventV1, CancellationToken>((callbackInvitationEventMessage, cancellationToken) =>
                {
                    invitationEventMessage = callbackInvitationEventMessage;
                });

            // Act
            await _dut.Handle(_command, default);

            // Assert
            _integrationEventPublisherMock.Verify(t => t.PublishAsync(It.IsAny<IInvitationEventV1>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual("A Invitation message description", invitationEventMessage.Description);

        }
    }
}
