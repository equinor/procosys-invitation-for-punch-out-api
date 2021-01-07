using Equinor.ProCoSys.IPO.Query.GetHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetHistory
{
    [TestClass]
    public class GetHistoryQueryTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            // Act 
            var dut = new GetHistoryQuery(1);

            // Assert
            Assert.AreEqual(1, dut.InvitationId);
        }
    }
}
