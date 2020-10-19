using Equinor.ProCoSys.IPO.Query.GetPersons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetPersons
{
    [TestClass]
    public class GetPersonsQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetPersonsQuery("A");

            Assert.AreEqual("A", dut.SearchString);
        }
    }
}
