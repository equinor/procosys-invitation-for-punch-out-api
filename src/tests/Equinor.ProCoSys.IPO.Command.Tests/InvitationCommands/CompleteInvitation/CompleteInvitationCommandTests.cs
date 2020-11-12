using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CompleteInvitation
{
    [TestClass]
    public class CompleteInvitationCommandTests
    {
        private const string InvitationRowVersion = "AAAAAAAAABA=";
        private const string ParticipantRowVersion = "AAAAAAAAABB=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new CompleteInvitationCommand(
                1,
                InvitationRowVersion,
                ParticipantRowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(InvitationRowVersion, dut.InvitationRowVersion);
            Assert.AreEqual(ParticipantRowVersion, dut.ParticipantRowVersion);
        }
    }
}
