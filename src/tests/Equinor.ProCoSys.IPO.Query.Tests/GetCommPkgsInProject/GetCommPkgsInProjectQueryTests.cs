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
            var dut = new GetCommPkgsInProjectQuery("ProjectName", "CommPkgNo");

            Assert.AreEqual("ProjectName", dut.ProjectName);
            Assert.AreEqual("CommPkgNo", dut.StartsWithCommPkgNo);
        }
    }
}
