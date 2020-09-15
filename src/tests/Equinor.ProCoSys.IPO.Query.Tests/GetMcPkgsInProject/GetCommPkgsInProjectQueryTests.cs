using Equinor.ProCoSys.IPO.Query.GetMcPkgsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetMcPkgsInProject
{
    [TestClass]
    public class SearchMcPkgsByMcPkgNoQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetMcPkgsInProjectQuery(2, "McPkgNo");

            Assert.AreEqual(2, dut.ProjectId);
            Assert.AreEqual("McPkgNo", dut.StartsWithMcPkgNo);
        }
    }
}
