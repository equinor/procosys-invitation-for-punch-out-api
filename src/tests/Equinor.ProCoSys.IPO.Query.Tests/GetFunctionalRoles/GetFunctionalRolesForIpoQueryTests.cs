using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetFunctionalRoles
{
    [TestClass]
    public class GetFunctionalRolesForIpoQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            const string Classification = "IPO";
            var dut = new GetFunctionalRolesForIpoQuery(Classification);

            Assert.AreEqual(Classification, dut.Classification);
        }
    }
}
