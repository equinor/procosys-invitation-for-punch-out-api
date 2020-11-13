using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatus;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatuses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.ChangeAttendedStatuses
{
    [TestClass]
    public class ChangeAttendedStatusesCommandTests
    {
        private const string InvitationRowVersion = "AAAAAAAAABA=";
        private const string ParticipantRowVersion = "AAAAAAAAABB=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new ChangeAttendedStatusesCommand(
                1,
                InvitationRowVersion,
                new List<ParticipantToChangeAttendedStatusForCommand>
                {
                    new ParticipantToChangeAttendedStatusForCommand(2, true, ParticipantRowVersion)
                });

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(InvitationRowVersion, dut.InvitationRowVersion);
            Assert.AreEqual(1, dut.Participants.Count);
            Assert.AreEqual(ParticipantRowVersion, dut.Participants.First().RowVersion);
            Assert.AreEqual(2, dut.Participants.First().Id);
            Assert.AreEqual(true, dut.Participants.First().Attended);
        }
    }
}
