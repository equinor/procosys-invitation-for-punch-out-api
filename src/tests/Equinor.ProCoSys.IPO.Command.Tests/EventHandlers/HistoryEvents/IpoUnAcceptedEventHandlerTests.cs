﻿using System;
using Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.HistoryEvents
{
    [TestClass]
    public class IpoUnAcceptedEventHandlerTests
    {
        private Mock<IHistoryRepository> _historyRepositoryMock;
        private IpoUnAcceptedEventHandler _dut;
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

            _dut = new IpoUnAcceptedEventHandler(_historyRepositoryMock.Object);
        }

        [TestMethod]
        public void Handle_ShouldAddIpoUnAcceptedHistoryRecord()
        {
            // Arrange
            Assert.IsNull(_historyAdded);

            // Act
            var objectGuid = Guid.NewGuid();
            var plant = "TestPlant";
            _dut.Handle(new IpoUnAcceptedEvent(plant, objectGuid), default);

            // Assert
            Assert.IsNotNull(_historyAdded);
            Assert.AreEqual(plant, _historyAdded.Plant);
            Assert.AreEqual(objectGuid, _historyAdded.ObjectGuid);
            Assert.IsNotNull(_historyAdded.Description);
            Assert.AreEqual(EventType.IpoUnaccepted, _historyAdded.EventType);
            Assert.AreEqual("IPO", _historyAdded.ObjectType);
        }
    }
}
