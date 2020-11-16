using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands
{
    [TestClass]
    public class UpdateAttendedStatusAndNotesOnParticipantsForCommandTests
    {
        private const string _note = "Test note";
        private const string _participantRowVersion = "AAAAAAAAABB=";

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new UpdateAttendedStatusAndNotesOnParticipantsForCommand(
                1,
                true,
                _note,
                _participantRowVersion);

            Assert.AreEqual(1, dut.Id);
            Assert.AreEqual(true, dut.Attended);
            Assert.AreEqual(_note, dut.Note);
            Assert.AreEqual(_participantRowVersion, dut.RowVersion);
        }
    }
}
