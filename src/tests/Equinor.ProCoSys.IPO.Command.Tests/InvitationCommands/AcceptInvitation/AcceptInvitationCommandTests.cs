using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AcceptInvitation
{
    [TestClass]
    public class AcceptInvitationCommandTests
    {
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _constructionCompanyRowVersion = "AAAAAAAAABB=";
        private const int _contractorParticipantId = 20;
        private const int _constructionCompanyParticipantId = 30;
        private const string _note = "Test note";
        private const string _participantRowVersion1 = "AAAAAAAAAYB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateNoteOnParticipantForCommand> _participants = new List<UpdateNoteOnParticipantForCommand>
        {
            new UpdateNoteOnParticipantForCommand(
                _contractorParticipantId,
                _note,
                _participantRowVersion1),
            new UpdateNoteOnParticipantForCommand(
                _constructionCompanyParticipantId,
                _note,
                _participantRowVersion2)
        };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new AcceptInvitationCommand(
                1,
                _invitationRowVersion,
                _constructionCompanyRowVersion,
                _participants);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(_invitationRowVersion, dut.InvitationRowVersion);
            Assert.AreEqual(_constructionCompanyRowVersion, dut.ParticipantRowVersion);
            Assert.AreEqual(2, dut.Participants.Count);
        }
    }
}
