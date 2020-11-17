using Equinor.ProCoSys.IPO.Query.GetAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetAttachments
{
    [TestClass]
    public class GetAttachmentsQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetAttachmentsQuery(1);

            Assert.AreEqual(1, dut.InvitationId);
        }
    }
}
