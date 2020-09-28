using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetFunctionalRoles
{
    [TestClass]
    public class GetFunctionalRolesQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            const string Classification = "classification";
            var dut = new GetFunctionalRolesQuery(Classification);

            Assert.AreEqual(Classification, dut.Classification);
        }
    }
}
