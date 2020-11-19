using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsByCommPkgNo
{
    [TestClass]
    public class GetInvitationsByCommPkgNoQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetInvitationsByCommPkgNoQuery("CommPkgNo", "ProjectName");

            Assert.AreEqual("CommPkgNo", dut.CommPkgNo);
            Assert.AreEqual("ProjectName", dut.ProjectName);
        }
    }
}
