using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CompleteInvitation
{
    [TestClass]
    public class CompleteInvitationCommandTests
    {
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _contractorRowVersion = "AAAAAAAAABB=";
        private const int _contractorParticipantId = 20;
        private const int _constructionCompanyParticipantId = 30;
        private const string _note = "Test note";
        private const string _participantRowVersion1 = "AAAAAAAAAYB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateAttendedStatusAndNotesOnParticipantsForCommand> _participants = new List<UpdateAttendedStatusAndNotesOnParticipantsForCommand>
        {
            new UpdateAttendedStatusAndNotesOnParticipantsForCommand(
                _contractorParticipantId,
                true,
                _note,
                _participantRowVersion1),
            new UpdateAttendedStatusAndNotesOnParticipantsForCommand(
                _constructionCompanyParticipantId,
                false,
                _note,
                _participantRowVersion2)
        };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new CompleteInvitationCommand(
                1,
                _invitationRowVersion,
                _contractorRowVersion,
                _participants);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(_invitationRowVersion, dut.InvitationRowVersion);
            Assert.AreEqual(_contractorRowVersion, dut.ParticipantRowVersion);
            Assert.AreEqual(2, dut.Participants.Count);
        }
    }
}
