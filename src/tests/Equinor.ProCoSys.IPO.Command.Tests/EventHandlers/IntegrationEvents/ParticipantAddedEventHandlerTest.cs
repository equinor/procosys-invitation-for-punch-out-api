using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents;
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.IPO.MessageContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.IntegrationEvents
{
    [TestClass]
    public class ParticipantAddedEventHandlerTest
    {
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;
        private Mock<ICreateEventHelper> _createEventHelperMock;
        private ParticipantAddedEventHandler _dut;
        private History _historyAdded;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();
            _createEventHelperMock = new Mock<ICreateEventHelper>();

            _dut = new ParticipantAddedEventHandler(_integrationEventPublisherMock.Object, _createEventHelperMock.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldNotPublishWhenParticipantIsPersonAsPartOfFunctionalRole()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            var plant = "TestPlant";
            var functionalRole = "A functional role";
            var participant = new Participant(plant, Organization.Commissioning, IpoParticipantType.Person,
                functionalRole, "John", "Smith", "john@equinor.com", "john@equinor.com", null, 1);

            // Act
            await _dut.Handle(new ParticipantAddedEvent(plant, sourceGuid, null, participant), default);

            // Assert
            _integrationEventPublisherMock
                .Verify(x => x.PublishAsync(It.IsAny<IParticipantEventV1>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ShouldPublishWhenParticipantIsPersonNotInFunctionalRole()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            var plant = "TestPlant";

            var participant = new Participant(plant, Organization.Commissioning, IpoParticipantType.Person,
                null, "John", "Smith", "john@equinor.com", "john@equinor.com", null, 1);

            // Act
            await _dut.Handle(new ParticipantAddedEvent(plant, sourceGuid, null, participant), default);

            // Assert
            _integrationEventPublisherMock
                .Verify(x => x.PublishAsync(It.IsAny<IParticipantEventV1>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
