using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.PcsBus.Receiver;
using Equinor.ProCoSys.PcsBus.Receiver.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.PcsBusTests
{
    [TestClass]
    public class PcsBusReceiverTests
    {
        private PcsBusReceiver _dut;
        private Mock<IPcsSubscriptionClients> _clients;
        MessageHandlerOptions _options;
        private Mock<ILogger<PcsBusReceiver>> _logger;
        private Mock<IBusReceiverService> _busReceiverService;

        [TestInitialize]
        public void Setup()
        {
            _logger = new Mock<ILogger<PcsBusReceiver>>();
            
            _clients = new Mock<IPcsSubscriptionClients>();
            _clients.Setup(c
                => c.RegisterPcsMessageHandler(
                    It.IsAny<Func<IPcsSubscriptionClient, Message, CancellationToken, Task>>(),
                    It.IsAny<MessageHandlerOptions>())
            ).Callback<Func<IPcsSubscriptionClient, Message, CancellationToken, Task> , MessageHandlerOptions>((s,options) => _options = options);
            
            _busReceiverService = new Mock<IBusReceiverService>();

            _dut = new PcsBusReceiver(_logger.Object, _clients.Object, new SingletonBusReceiverServiceFactory(_busReceiverService.Object));
        }

        [TestMethod]
        public void StopAsync_ShouldCallCloseAllAsyncOnce()
        {
            _dut.StopAsync(new CancellationToken());

            _clients.Verify(c => c.CloseAllAsync(), Times.Once );
        }

        [TestMethod]
        public void StartAsync_ShouldVerifyRegisterOcsMessageHandlerWasCalledAndMaxConcurrentCallsWasSet()
        {
            _dut.StartAsync(new CancellationToken());

            _clients.Verify(c => c.RegisterPcsMessageHandler(It.IsAny<Func<IPcsSubscriptionClient, Message, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()));
            Assert.AreEqual(1, _options.MaxConcurrentCalls);
        }

        [TestMethod]
        public async Task ProcessMessageAsync_ShouldCallProcessMessageAsync()
        {
            var client = new Mock<IPcsSubscriptionClient>();
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"asdf\", \"ProjectName\" : \"ew2f\", \"Description\" : \"sdf\"}}"));
            var lockToken = Guid.NewGuid();

            SetLockToken(message, lockToken);
            await _dut.ProcessMessagesAsync(client.Object, message, new CancellationToken());

            _busReceiverService.Verify(b => b.ProcessMessageAsync(client.Object.PcsTopic, message, It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.CompleteAsync(lockToken.ToString()), Times.Once);
        }

        private static void SetLockToken(Message message, Guid lockToken)
        {
            var systemProperties = message.SystemProperties;
            var type = systemProperties.GetType();
            type.GetMethod("set_LockTokenGuid", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(systemProperties, new object[] {lockToken});
            type.GetMethod("set_SequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(systemProperties, new object[] {0});
        }
    }
}
