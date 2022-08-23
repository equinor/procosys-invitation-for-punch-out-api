using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateNoteOnParticipant;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateNoteOnParticipant
{
    [TestClass]
    public class UpdateNoteOnParticipantCommandTests
    {
        private const string ParticipantRowVersion = "AAAAAAAAABB=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new UpdateNoteOnParticipantCommand(
                1,
                2,
                "note",
                ParticipantRowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(ParticipantRowVersion, dut.RowVersion);
            Assert.AreEqual(2, dut.ParticipantId);
            Assert.AreEqual("note", dut.Note);
        }
    }
}
