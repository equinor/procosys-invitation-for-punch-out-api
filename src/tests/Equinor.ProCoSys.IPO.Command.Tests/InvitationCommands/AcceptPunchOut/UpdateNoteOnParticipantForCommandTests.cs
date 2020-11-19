using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AcceptPunchOut
{
    [TestClass]
    public class UpdateNoteOnParticipantForCommandTests
    {
        private const string _note = "Test note";
        private const string _participantRowVersion = "AAAAAAAAABB=";

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new UpdateNoteOnParticipantForCommand(
                1,
                _note,
                _participantRowVersion);

            Assert.AreEqual(1, dut.Id);
            Assert.AreEqual(_note, dut.Note);
            Assert.AreEqual(_participantRowVersion, dut.RowVersion);
        }
    }
}
