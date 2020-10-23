using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.DeleteAttachment
{
    [TestClass]
    public class DeleteAttachmentCommandTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new DeleteAttachmentCommand(1, 2, "3");

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(2, dut.AttachmentId);
            Assert.AreEqual("3", dut.RowVersion);
        }
    }
}
