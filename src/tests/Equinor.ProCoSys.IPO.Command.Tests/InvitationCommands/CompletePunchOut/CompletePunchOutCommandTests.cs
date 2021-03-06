﻿using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CompletePunchOut
{
    [TestClass]
    public class CompletePunchOutCommandTests
    {
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _contractorRowVersion = "AAAAAAAAABB=";
        private const int _contractorParticipantId = 20;
        private const int _constructionCompanyParticipantId = 30;
        private const string _note = "Test note";
        private const string _participantRowVersion1 = "AAAAAAAAAYB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateAttendedStatusAndNoteOnParticipantForCommand> _participants = new List<UpdateAttendedStatusAndNoteOnParticipantForCommand>
        {
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _contractorParticipantId,
                true,
                _note,
                _participantRowVersion1),
            new UpdateAttendedStatusAndNoteOnParticipantForCommand(
                _constructionCompanyParticipantId,
                false,
                _note,
                _participantRowVersion2)
        };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new CompletePunchOutCommand(
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
