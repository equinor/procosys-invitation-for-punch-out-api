using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetPersonsInUserGroup
{
    [TestClass]
    public class GetPersonsInUserGroupQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetPersonsWithPrivilegesQuery("A", "IPO", new List<string> {"READ"});

            Assert.AreEqual("A", dut.SearchString);
            Assert.AreEqual("IPO", dut.ObjectName);
            Assert.AreEqual(1, dut.Privileges.Count);
            Assert.AreEqual("READ", dut.Privileges.First());
        }
    }
}
