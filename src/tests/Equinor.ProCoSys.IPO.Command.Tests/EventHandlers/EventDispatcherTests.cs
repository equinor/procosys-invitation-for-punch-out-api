﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Command.EventHandlers;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers
{
    [TestClass]
    public class EventDispatcherTests
    {
        [TestMethod]
        public void Constructor_ThrowsException_IFMediatorIsNull_Test() 
            => Assert.ThrowsException<ArgumentNullException>(() => new EventDispatcher(null));

        [TestMethod]
        public async Task DispatchPreSaveAsync_SendsOutEvents_Test()
        {
            var mediator = new Mock<IMediator>();
            var dut = new EventDispatcher(mediator.Object);
            var entities = new List<TestableEntityBase>();

            for (var i = 0; i < 3; i++)
            {
                var entity = new Mock<TestableEntityBase>();
                entity.Object.AddPreSaveDomainEvent(new Mock<INotification>().Object);
                entity.Object.AddPostSaveDomainEvent(new Mock<INotification>().Object);
                entities.Add(entity.Object);
            }
            await dut.DispatchPreSaveAsync(entities);

            mediator.Verify(x 
                => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

            entities.ForEach(e => Assert.AreEqual(0, e.PreSaveDomainEvents.Count));
            entities.ForEach(e => Assert.AreEqual(1, e.PostSaveDomainEvents.Count));
        }

        [TestMethod]
        public async Task DispatchPostSaveAsync_SendsOutEvents_Test()
        {
            var mediator = new Mock<IMediator>();
            var dut = new EventDispatcher(mediator.Object);
            var entities = new List<TestableEntityBase>();

            for (var i = 0; i < 3; i++)
            {
                var entity = new Mock<TestableEntityBase>();
                entity.Object.AddPreSaveDomainEvent(new Mock<INotification>().Object);
                entity.Object.AddPostSaveDomainEvent(new Mock<INotification>().Object);
                entities.Add(entity.Object);
            }
            await dut.DispatchPostSaveAsync(entities);

            mediator.Verify(x
                => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

            entities.ForEach(e => Assert.AreEqual(1, e.PreSaveDomainEvents.Count));
            entities.ForEach(e => Assert.AreEqual(0, e.PostSaveDomainEvents.Count));
        }

        [TestMethod]
        public async Task DispatchPreSaveAsync_ThrowsException_IfListIsEmptyAsync_Test()
        {
            var mediator = new Mock<IMediator>();
            var dut = new EventDispatcher(mediator.Object);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                dut.DispatchPreSaveAsync(null));
        }

        [TestMethod]
        public async Task DispatchPostSaveAsync_ThrowsException_IfListIsEmptyAsync_Test()
        {
            var mediator = new Mock<IMediator>();
            var dut = new EventDispatcher(mediator.Object);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                dut.DispatchPostSaveAsync(null));
        }
    }

    public class TestableEntityBase : EntityBase
    {
        // The base class is abstract, therefor a sub class is needed to test it.
    }

}
