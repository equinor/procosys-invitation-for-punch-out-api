//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Azure.Messaging.ServiceBus;
//using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
//using Equinor.ProCoSys.PcsServiceBus.Sender;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;

//namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.PostSaveEvents
//{
//    [TestClass]
//    public class IpoUnAcceptedEventHandlerTests
//    {
//        private IpoUnAcceptedEventHandler _dut;
//        private Mock<ServiceBusSender> _serviceBusSender;
//        private PcsBusSender _pcsBusSender;

//        [TestInitialize]
//        public void Setup()
//        {
//            _serviceBusSender = new Mock<ServiceBusSender>();
//            _pcsBusSender = new PcsBusSender();
//            _pcsBusSender.Add("ipo", _serviceBusSender.Object);
//            _dut = new IpoUnAcceptedEventHandler(_pcsBusSender);
//        }

//        [TestMethod]
//        public async Task Handle_ShouldSendBusTopic()
//        {
//            // Arrange
//            var sourceGuid = Guid.NewGuid();
//            const string Plant = "TestPlant";
//            var ipoUnAcceptedEvent = new IpoUnAcceptedEvent(Plant, sourceGuid);

//            // Act
//            await _dut.Handle(ipoUnAcceptedEvent, default);

//            // Assert
//            _serviceBusSender.Verify(t => t.SendMessageAsync(It.IsAny<ServiceBusMessage>(),
//                It.IsAny<CancellationToken>()), Times.Once());
//        }
//    }
//}
