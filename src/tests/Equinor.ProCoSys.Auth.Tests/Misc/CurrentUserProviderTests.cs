using System;
using Equinor.ProCoSys.Auth.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Auth.Tests.Misc
{
    [TestClass]
    public class CurrentUserProviderTests
    {
        private readonly Guid _okOid = new Guid("7DFC890F-F82B-4E2D-B81B-41D6C103F83B");

        [TestMethod]
        public void GetCurrentUserOid_ShouldReturnOid_WhenOidExists()
        {
            var dut = new CurrentUserProvider();
            dut.SetCurrentUserOid(_okOid);

            var oid = dut.GetCurrentUserOid();

            Assert.AreEqual(_okOid, oid);
        }

        [TestMethod]
        public void GetCurrentUserOid_ThrowsException_WhenOidDoesNotExist()
        {
            var dut = new CurrentUserProvider();
            Assert.ThrowsException<Exception>(() => dut.GetCurrentUserOid());
        }

        [TestMethod]
        public void HasCurrentUser_ShouldReturnFalse_WhenOidDoesNotExist()
        {
            var dut = new CurrentUserProvider();

            Assert.IsFalse(dut.HasCurrentUser);
        }

        [TestMethod]
        public void HasCurrentUser_ShouldReturnTrue_WhenOidDoesExist()
        {
            var dut = new CurrentUserProvider();
            dut.SetCurrentUserOid(_okOid);

            Assert.IsTrue(dut.HasCurrentUser);
        }
    }
}
