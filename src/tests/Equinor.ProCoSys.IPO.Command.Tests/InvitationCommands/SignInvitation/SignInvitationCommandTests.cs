using Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.SignInvitation
{
    [TestClass]
    public class SignInvitationCommandTests
    {
        private const string _participantRowVersion = "AAAAAAAAABB=";
        private const int _participantId = 20;


        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new SignInvitationCommand(
                1,
                _participantId,
                _participantRowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(_participantRowVersion, dut.ParticipantRowVersion);
            Assert.AreEqual(_participantId, dut.ParticipantId);
        }
    }
}
