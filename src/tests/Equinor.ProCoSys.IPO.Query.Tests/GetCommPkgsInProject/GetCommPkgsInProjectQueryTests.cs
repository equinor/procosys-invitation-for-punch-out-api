using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetCommPkgsInProject
{
    [TestClass]
    public class SearchCommPkgsByCommPkgNoQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetCommPkgsInProjectQuery("ProjectName", "CommPkgNo", 10, 0);

            Assert.AreEqual("ProjectName", dut.ProjectName);
            Assert.AreEqual("CommPkgNo", dut.StartsWithCommPkgNo);
            Assert.AreEqual(10, dut.ItemsPerPage);
            Assert.AreEqual(0, dut.CurrentPage);
        }
    }
}
