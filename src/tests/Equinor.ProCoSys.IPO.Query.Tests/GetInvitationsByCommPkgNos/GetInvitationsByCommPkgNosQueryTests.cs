using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsByCommPkgNos
{
    [TestClass]
    public class GetInvitationsByCommPkgNosQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetInvitationsByCommPkgNosQuery(new List<string> {"CommPkgNo", "Another"}, "ProjectName");

            Assert.IsNotNull(dut.CommPkgNos);
            Assert.AreEqual("CommPkgNo", dut.CommPkgNos.First());
            Assert.AreEqual("Another", dut.CommPkgNos.Last());
            Assert.AreEqual("ProjectName", dut.ProjectName);
        }
    }
}
