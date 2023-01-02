using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetLatestMdpIpoStatusOnCommPkgs
{
    [TestClass]
    public class GetLatestMdpIpoStatusOnCommPkgsQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { "CommPkgNo", "Another" }, "ProjectName");

            Assert.IsNotNull(dut.CommPkgNos);
            Assert.AreEqual(2, dut.CommPkgNos.Count());
            Assert.AreEqual("CommPkgNo", dut.CommPkgNos.First());
            Assert.AreEqual("Another", dut.CommPkgNos.Last());
            Assert.AreEqual("ProjectName", dut.ProjectName);
        }

        [TestMethod]
        public void Constructor_StripBlank_CommPkgNos()
        {
            var dut = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { "CommPkgNo", null, "", "Another" }, "ProjectName");

            Assert.IsNotNull(dut.CommPkgNos);
            Assert.AreEqual(2, dut.CommPkgNos.Count());
            Assert.AreEqual("CommPkgNo", dut.CommPkgNos.First());
            Assert.AreEqual("Another", dut.CommPkgNos.Last());
            Assert.AreEqual("ProjectName", dut.ProjectName);
        }
    }
}
