using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetOutstandingIpos
{
    [TestClass]
    public class GetOutstandingIposQueryTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            // Act 
            var dut = new GetOutstandingIposForCurrentPersonQuery("ProjectName");

            // Assert
            Assert.AreEqual("ProjectName", dut.ProjectName);
        }
    }
}
