using Equinor.ProCoSys.IPO.Query.GetComments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetComments
{
    [TestClass]
    public class GetCommentsQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetCommentsQuery(1);

            Assert.AreEqual(1, dut.InvitationId);
        }
    }
}
