using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventHandlers.LoggingEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.LoggingEvents
{
    [TestClass]
    public class IpoNotSetToHandedOverEventHandlerTests
    {
        private Mock<ILogger<IpoNotSetToHandedOverEventHandler>> _loggerMock;
        private IpoNotSetToHandedOverEventHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _loggerMock = new Mock<ILogger<IpoNotSetToHandedOverEventHandler>>();

            _dut = new IpoNotSetToHandedOverEventHandler(_loggerMock.Object);
        }

        [TestMethod]
        public void Handle_ShouldAddIpoCreatedHistoryRecord()
        {
            //Arrange
            var plant = "TestPlant";
            var guid = new Guid();
            var status = IpoStatus.Accepted;
            Func<object, Type, bool> state = (v, t) => v.ToString()
                .CompareTo($"{EventType.IpoNotHandedOver.GetDescription()}. Plant: [{plant}], Guid [{guid}], " +
                           $"Current status: [{status}].") == 0;

            // Act
            _dut.Handle(new IpoNotSetToHandedOverEvent(plant, guid, status), default);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
