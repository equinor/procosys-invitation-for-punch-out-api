using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.IPO.MessageContracts;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.IntegrationEvents
{
    [TestClass]
    public class ParticipantUpdatedEventHandlerTest
    {
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ILogger<CreateEventHelper>> _loggerMock;

        private CreateEventHelper _createEventHelper;
        private Project _project;
        public McPkg _mcPkg;
        private readonly string _plant = "TestPlant";
        private Invitation _invitation;
        private ParticipantUpdatedEventHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _project = new Project("TestPlant", "TestProject", "TestDescription", Guid.NewGuid());
            _mcPkg = new McPkg(_plant, _project, "123", "123-45", "description", "3|4", Guid.NewGuid(),
                Guid.NewGuid());
            _invitation = new Invitation(_plant, _project, "Title", "Description", DisciplineType.DP, DateTime.MinValue,
                DateTime.MinValue, "Location", new List<McPkg> { _mcPkg }, new List<CommPkg>());
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(_project);
            
            _personRepositoryMock = new Mock<IPersonRepository>();

            _loggerMock = new Mock<ILogger<CreateEventHelper>>();
            _createEventHelper = new CreateEventHelper(_projectRepositoryMock.Object, _personRepositoryMock.Object, _loggerMock.Object);;
            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();

            _dut = new ParticipantUpdatedEventHandler(_integrationEventPublisherMock.Object, _createEventHelper);
        }

        [TestMethod]
        public async Task Handle_ShouldNotPublishWhenParticipantIsPersonAsPartOfFunctionalRole()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            var functionalRole = "A functional role";

            
            var participant = new Participant(_plant, Organization.Commissioning, IpoParticipantType.Person,
                functionalRole, "John", "Smith", "john@equinor.com", "john@equinor.com", null, 1);


            // Act
            await _dut.Handle(new ParticipantUpdatedEvent(_plant, sourceGuid, _invitation, participant), default);

            // Assert
            _integrationEventPublisherMock
                .Verify(x => x.PublishAsync(It.IsAny<IParticipantEventV1>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ShouldPublishWhenParticipantIsPersonNotInFunctionalRole()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();

            var participant = new Participant(_plant, Organization.Commissioning, IpoParticipantType.Person,
                null, "John", "Smith", "john@equinor.com", "john@equinor.com", null, 1);

            // Act
            await _dut.Handle(new ParticipantUpdatedEvent(_plant, sourceGuid, _invitation, participant), default);

            // Assert
            _integrationEventPublisherMock
                .Verify(x => x.PublishAsync(It.IsAny<IParticipantEventV1>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldPublishWhenParticipantIsOfTypeFunctionalRole()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            var functionalRole = "A functional role";

            var participant = new Participant(_plant, Organization.Commissioning, IpoParticipantType.FunctionalRole,
                functionalRole, null, null, null, null, null, 1);

            // Act
            await _dut.Handle(new ParticipantUpdatedEvent(_plant, sourceGuid, _invitation, participant), default);

            // Assert
            _integrationEventPublisherMock
                .Verify(x => x.PublishAsync(It.IsAny<IParticipantEventV1>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
