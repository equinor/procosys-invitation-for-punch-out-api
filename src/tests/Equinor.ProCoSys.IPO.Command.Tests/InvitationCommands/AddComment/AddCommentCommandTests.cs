using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AddComment
{
    [TestClass]
    public class AddCommentCommandTests
    {
        private const string _comment = "comment text";

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new AddCommentCommand(
                1,
                _comment);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(_comment, dut.Comment);
        }
    }
}
