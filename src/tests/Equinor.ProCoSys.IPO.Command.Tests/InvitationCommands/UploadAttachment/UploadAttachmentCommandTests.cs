using System.IO;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UploadAttachment
{
    [TestClass]
    public class UploadAttachmentCommandTests
    {
        [TestMethod]
        public void Cosntructor_SetsProperties()
        {
            var stream = new MemoryStream();
            var dut = new UploadAttachmentCommand(1, "FileName.txt", true, stream);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual("FileName.txt", dut.FileName);
            Assert.AreEqual(true, dut.OverWriteIfExists);
            Assert.AreEqual(stream, dut.Content);
        }
    }
}
