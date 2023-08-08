using System;
using Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.HistoryEvents
{
    [TestClass]
    public class AttendedStatusUpdatedEventHandlerTests
    {
        private Mock<IHistoryRepository> _historyRepositoryMock;
        private AttendedStatusUpdatedEventHandler _dut;
        private History _historyAdded;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _historyRepositoryMock = new Mock<IHistoryRepository>();
            _historyRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<History>()))
                .Callback<History>(history =>
                {
                    _historyAdded = history;
                });

            _dut = new AttendedStatusUpdatedEventHandler(_historyRepositoryMock.Object);
        }

        [TestMethod]
        public void Handle_ShouldUpdateAttendedStatusUpdatedHistoryRecord()
        {
            // Arrange
            Assert.IsNull(_historyAdded);

            // Act
            var sourceGuid = Guid.NewGuid();
            var plant = "TestPlant";
            var participant = new Participant("TestPlant", 
                                    Domain.AggregateModels.InvitationAggregate.Organization.ConstructionCompany, 
                                    IpoParticipantType.Person, 
                                    null,
                                    "Rob", 
                                    "Hubbard",
                                    "robhubbard",
                                    "a@b.com", 
                                    sourceGuid, 
                                    0);

            _dut.Handle(new AttendedStatusUpdatedEvent(plant, sourceGuid), default);

            // Assert
            Assert.IsNotNull(_historyAdded);
            Assert.AreEqual(plant, _historyAdded.Plant);
            Assert.AreEqual(sourceGuid, _historyAdded.SourceGuid);
            Assert.IsNotNull(_historyAdded.Description);
            Assert.AreEqual(EventType.AttendedStatusUpdated, _historyAdded.EventType);
            Assert.AreEqual("IPO", _historyAdded.ObjectType);
        }
    }
}
