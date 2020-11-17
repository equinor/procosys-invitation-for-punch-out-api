using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    [TestClass]
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandTests
    {
        private const string ParticipantRowVersion = "AAAAAAAAABB=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new UpdateAttendedStatusAndNotesOnParticipantsCommand(
                1,
                new List<UpdateAttendedStatusAndNoteOnParticipantForCommand>
                {
                    new UpdateAttendedStatusAndNoteOnParticipantForCommand(2, true, "note", ParticipantRowVersion)
                });

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(1, dut.Participants.Count);
            Assert.AreEqual(ParticipantRowVersion, dut.Participants.First().RowVersion);
            Assert.AreEqual(2, dut.Participants.First().Id);
            Assert.AreEqual("note", dut.Participants.First().Note);
            Assert.AreEqual(true, dut.Participants.First().Attended);
        }
    }
}
