using Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetMcPkgsUnderCommPkgInProject
{
    [TestClass]
    public class GetMcPkgsUnderCommPkgInProjectQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetMcPkgsUnderCommPkgInProjectQuery("Pname", "CommPkgNo");

            Assert.AreEqual("Pname", dut.ProjectName);
        }
    }
}
