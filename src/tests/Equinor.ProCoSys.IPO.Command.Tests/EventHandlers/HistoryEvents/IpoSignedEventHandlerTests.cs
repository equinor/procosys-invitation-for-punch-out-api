using System;
using Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.HistoryEvents
{
    [TestClass]
    public class IpoSignedEventHandlerTests
    {
        private Mock<IHistoryRepository> _historyRepositoryMock;
        private IpoSignedEventHandler _dut;
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

            _dut = new IpoSignedEventHandler(_historyRepositoryMock.Object);
        }

        [TestMethod]
        public void Handle_ShouldAddIpoSignedHistoryRecord()
        {
            // Arrange
            Assert.IsNull(_historyAdded);

            // Act
            var objectGuid = Guid.NewGuid();
            var plant = "TestPlant";
            _dut.Handle(new IpoSignedEvent(plant, objectGuid), default);

            // Assert
            Assert.IsNotNull(_historyAdded);
            Assert.AreEqual(plant, _historyAdded.Plant);
            Assert.AreEqual(objectGuid, _historyAdded.ObjectGuid);
            Assert.IsNotNull(_historyAdded.Description);
            Assert.AreEqual(EventType.IpoSigned, _historyAdded.EventType);
            Assert.AreEqual("IPO", _historyAdded.ObjectType);
        }
    }
}
