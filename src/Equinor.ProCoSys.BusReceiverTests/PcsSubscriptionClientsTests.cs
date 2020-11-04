using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.BusReceiverTests
{
    [TestClass]
    public class PcsSubscriptionClientsTests
    {
        private PcsSubscriptionClients _dut;
        private Mock<IPcsSubscriptionClient> _client1;
        private Mock<IPcsSubscriptionClient> _client2;

        [TestInitialize]
        public void Setup()
        {
            _dut = new PcsSubscriptionClients();
            _client1 = new Mock<IPcsSubscriptionClient>();
            _client2 = new Mock<IPcsSubscriptionClient>();

            _dut.Add(_client1.Object);
            _dut.Add(_client2.Object);
        }

        [TestMethod]
        public async Task CloseAllAsyncTestAsync()
        {
            await _dut.CloseAllAsync();
            _client1.Verify(c => c.CloseAsync(), Times.Once);
            _client2.Verify(c => c.CloseAsync(), Times.Once);
        }

        [TestMethod]
        public async Task AllMethodsWorkWithoutFailureOnEmpty()
        {
            var emptyClients = new PcsSubscriptionClients();
            var handler = new Mock<Func<IPcsSubscriptionClient, Message, CancellationToken, Task>>();
            var options = new MessageHandlerOptions(Test);
            await emptyClients.CloseAllAsync();
            emptyClients.RegisterPcsMessageHandler(handler.Object, options);
        }

        private Task Test(ExceptionReceivedEventArgs arg) => Task.CompletedTask;
    }
}
