using System;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsQueries.GetInvitations
{
    [TestClass]
    public class PagingTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new Paging(0, 5);
            Assert.AreEqual(0, dut.Page);
            Assert.AreEqual(5, dut.Size);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenNegativePage()
            => Assert.ThrowsException<ArgumentException>(() =>
                new Paging(-1, 5)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenZeroSize()
            => Assert.ThrowsException<ArgumentException>(() =>
                new Paging(0, 0)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenNegativeSize()
            => Assert.ThrowsException<ArgumentException>(() =>
                new Paging(0, -1)
            );
    }
}
