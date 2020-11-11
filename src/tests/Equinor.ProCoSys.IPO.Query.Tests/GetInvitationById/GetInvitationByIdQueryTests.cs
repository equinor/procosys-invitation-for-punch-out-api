using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationById
{
    [TestClass]
    public class GetInvitationByIdQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetInvitationByIdQuery(2);

            Assert.AreEqual(2, dut.InvitationId);
        }
    }
}
