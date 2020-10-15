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
            var dut = new GetPersonsInUserGroupQuery("A", "MC_LEAD");

            Assert.AreEqual("A", dut.SearchString);
            Assert.AreEqual("MC_LEAD", dut.UserGroup);
        }
    }
}
