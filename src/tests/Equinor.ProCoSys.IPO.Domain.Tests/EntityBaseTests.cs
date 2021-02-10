using System.Linq;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Domain.Tests
{
    [TestClass]
    public class EntityBaseTests
    {
        private readonly byte[] ConvertedRowVersion = {0, 0, 0, 0, 0, 0, 0, 16};
        private const string RowVersion = "AAAAAAAAABA=";

        [TestMethod]
        public void ReturningEmptyDomainEventsListTest()
        {
            var dut = new TestableEntityBase();
            Assert.IsNotNull(dut.PreSaveDomainEvents);
        }

        [TestMethod]
        public void DomainEventIsAddedToListTest()
        {
            var dut = new TestableEntityBase();
            var domainEvent = new Mock<INotification>();
            dut.AddPreSaveDomainEvent(domainEvent.Object);

            Assert.IsTrue(dut.PreSaveDomainEvents.Contains(domainEvent.Object));
        }

        [TestMethod]
        public void DomainEventIsRemovedFromListTest()
        {
            var dut = new TestableEntityBase();
            var domainEvent = new Mock<INotification>();
            dut.AddPreSaveDomainEvent(domainEvent.Object);
            dut.RemovePreSaveDomainEvent(domainEvent.Object);

            Assert.IsFalse(dut.PreSaveDomainEvents.Contains(domainEvent.Object));
        }

        [TestMethod]
        public void DomainEventsAreClearedTest()
        {
            var dut = new TestableEntityBase();
            var domainEvent1 = new Mock<INotification>();
            dut.AddPreSaveDomainEvent(domainEvent1.Object);
            var domainEvent2 = new Mock<INotification>();
            dut.AddPreSaveDomainEvent(domainEvent2.Object);

            dut.ClearPreSaveDomainEvents();

            Assert.AreEqual(0, dut.PreSaveDomainEvents.Count);
        }

        [TestMethod]
        public void GetRowVersion_ShouldReturnLastSetRowVersion()
        {
            var dut = new TestableEntityBase();
            Assert.IsNotNull(dut.RowVersion);
            dut.SetRowVersion(RowVersion);
            Assert.IsTrue(dut.RowVersion.SequenceEqual(ConvertedRowVersion));
        }
       
        private class TestableEntityBase : EntityBase
        {
            // The base class is abstract, therefor a sub class is needed to test it.
        }
    }
}
