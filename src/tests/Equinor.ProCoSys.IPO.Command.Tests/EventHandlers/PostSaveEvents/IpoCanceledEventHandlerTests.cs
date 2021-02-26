using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.PcsBus.Sender;
using Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.PostSaveEvents
{
    [TestClass]
    public class IpoCanceledEventHandlerTests
    {
        private IpoCanceledEventHandler _dut;
        private Mock<ITopicClient> _topicClient;
        private PcsBusSender _pcsBusSender;

        [TestInitialize]
        public void Setup()
        {
            _topicClient = new Mock<ITopicClient>();
            _pcsBusSender = new PcsBusSender();
            _pcsBusSender.Add("ipo", _topicClient.Object);
            _dut = new IpoCanceledEventHandler(_pcsBusSender);
        }

        [TestMethod]
        public async Task Handle_ShouldSendBusTopic()
        {
            // Arrange
            var objectGuid = Guid.NewGuid();
            var plant = "TestPlant";
            var ipoCanceledEvent = new IpoCanceledEvent(plant, objectGuid, IpoStatus.Canceled);

            // Act
            await _dut.Handle(ipoCanceledEvent, default);

            // Assert
            _topicClient.Verify(t => t.SendAsync(It.IsAny<Message>()), Times.Once());

        }
    }
}
