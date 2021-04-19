using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.PostSaveEvents
{
    [TestClass]
    public class IpoCompletedEventHandlerTests
    {
        private IpoCompletedEventHandler _dut;
        private Mock<ITopicClient> _topicClient;
        private PcsBusSender _pcsBusSender;

        [TestInitialize]
        public void Setup()
        {
            _topicClient = new Mock<ITopicClient>();
            _pcsBusSender = new PcsBusSender();
            _pcsBusSender.Add("ipo", _topicClient.Object);
            _dut = new IpoCompletedEventHandler(_pcsBusSender);
        }

        [TestMethod]
        public async Task Handle_ShouldSendBusTopic()
        {
            // Arrange
            var objectGuid = Guid.NewGuid();
            var plant = "PCS$TestPlant";
            var emails = new List<string>() {"email1@test.com", "email2@test.com"};
            var ipoCompletedEvent = new IpoCompletedEvent(plant, objectGuid, 1234, "Invitation title", emails);

            // Act
            await _dut.Handle(ipoCompletedEvent, default);

            // Assert
            _topicClient.Verify(t => t.SendAsync(It.IsAny<Message>()), Times.Once());
        }
    }
}
