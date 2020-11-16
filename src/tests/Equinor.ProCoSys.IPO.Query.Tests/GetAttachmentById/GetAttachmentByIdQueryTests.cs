using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetAttachmentById
{
    [TestClass]
    public class GetAttachmentByIdQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetAttachmentByIdQuery(1, 2);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(2, dut.AttachmentId);
        }
    }
}
