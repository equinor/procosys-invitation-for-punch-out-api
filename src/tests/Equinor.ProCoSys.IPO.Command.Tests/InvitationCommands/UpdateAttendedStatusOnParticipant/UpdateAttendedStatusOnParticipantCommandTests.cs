using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusOnParticipant;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateAttendedStatusOnParticipant
{
    [TestClass]
    public class UpdateAttendedStatusOnParticipantCommandTests
    {
        private const string ParticipantRowVersion = "AAAAAAAAABB=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new UpdateAttendedStatusOnParticipantCommand(
                1,
                2,
                true,
                ParticipantRowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(ParticipantRowVersion, dut.RowVersion);
            Assert.AreEqual(2, dut.ParticipantId);
            Assert.IsTrue(dut.Attended);
        }
    }
}
