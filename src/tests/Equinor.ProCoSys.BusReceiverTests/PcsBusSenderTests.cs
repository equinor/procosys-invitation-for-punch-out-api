using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.PcsBus.Receiver;
using Equinor.ProCoSys.PcsBus.Receiver.Interfaces;
using Equinor.ProCoSys.PcsBus.Sender;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace Equinor.ProCoSys.PcsBusTests
{
    [TestClass]
    public class PcsBusSenderTests
    {
        private PcsBusSender _dut;
        private Mock<ITopicClient> _topicClient1, _topicClient2;
        private const string _topicName1 = "Topic1";
        private const string _topicName2 = "Topic2";

        [TestInitialize]
        public void Setup()
        {
            _topicClient1 =  new Mock<ITopicClient>();
            _topicClient2 = new Mock<ITopicClient>();
            _dut = new PcsBusSender();
            
            _dut.Add(_topicName1, _topicClient1.Object);
            _dut.Add(_topicName2, _topicClient2.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task SendAsync_ShouldThrowExceptionIfTopicNotRegisteredTest()
        {
            // Act
            await _dut.SendAsync("AnUnknownTopic", new Message(Encoding.UTF8.GetBytes("Whatever")));
        }

        [TestMethod]
        public async Task SendAsync_ShouldOnlySendViaCorrectTopicClientTest()
        {
            // Arrange
            var message = new Message(Encoding.UTF8.GetBytes($@"{{One small {Guid.NewGuid()}}}"));

            // Act
            await _dut.SendAsync(_topicName1, message);

            // Assert
            _topicClient1.Verify(t => t.SendAsync(message), Times.Once);
            _topicClient2.Verify(t => t.SendAsync(message), Times.Never);
        }

        [TestMethod]
        public async Task CloseAll_ShouldCloseAllTopicClientsTest()
        {
            // Act
            await _dut.CloseAllAsync();

            // Assert
            _topicClient1.Verify(t => t.CloseAsync(), Times.Once);
            _topicClient2.Verify(t => t.CloseAsync(), Times.Once);
        }
    }
}
