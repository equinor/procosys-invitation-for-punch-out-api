using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender;
using Azure.Messaging.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.PostSaveEvents
{
    [TestClass]
    public class IpoAcceptedEventHandlerTests
    {
        private IpoAcceptedEventHandler _dut;
        private Mock<ServiceBusSender> _serviceBusSender;
        private PcsBusSender _pcsBusSender;

        [TestInitialize]
        public void Setup()
        {
            _serviceBusSender = new Mock<ServiceBusSender>();
            _pcsBusSender = new PcsBusSender();
            _pcsBusSender.Add("ipo", _serviceBusSender.Object);
            _dut = new IpoAcceptedEventHandler(_pcsBusSender);
        }

        [TestMethod]
        public async Task Handle_ShouldSendBusTopic()
        {
            // Arrange
            var objectGuid = Guid.NewGuid();
            const string Plant = "TestPlant";
            var ipoAcceptedEvent = new IpoAcceptedEvent(Plant, objectGuid);

            // Act
            await _dut.Handle(ipoAcceptedEvent, default);

            // Assert
            _serviceBusSender.Verify(t => t.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), 
                Times.Once());
        }
    }
}
