using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandTests
    {
        private readonly List<Guid> _requiredParticipantIds = new List<Guid>() { new Guid("22222222-3333-3333-3333-444444444444") };
        private readonly List<string> _requiredParticipantEmails = new List<string>() { "abc@example.com" };
        private readonly List<Guid> _optionalParticipantIds = new List<Guid>() { new Guid("33333333-4444-4444-4444-555555555555") };
        private readonly List<string> _optionalParticipantEmails = new List<string>() { "def@example.com" };

        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _type = "DP";

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var meeting = new CreateMeetingCommand(
                    "title",
                    "body",
                    "location",
                    new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                    _requiredParticipantIds,
                    _requiredParticipantEmails,
                    _optionalParticipantIds,
                    _optionalParticipantEmails);
            var dut = new CreateInvitationCommand(_title, _projectName, _type, meeting, null, null);

            Assert.AreEqual(meeting, dut.Meeting);
            Assert.AreEqual("title", dut.Meeting.Title);
            Assert.AreEqual("body", dut.Meeting.BodyHtml);
            Assert.AreEqual("location", dut.Meeting.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.Meeting.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.Meeting.EndTime);
            Assert.IsNotNull(dut.Meeting.RequiredParticipantOids);
            Assert.AreEqual(1, dut.Meeting.RequiredParticipantOids.Count());
            Assert.AreEqual(_requiredParticipantIds.First(), dut.Meeting.RequiredParticipantOids.First());
            Assert.AreEqual(_requiredParticipantEmails.First(), dut.Meeting.RequiredParticipantEmails.First());
            Assert.AreEqual(_optionalParticipantIds.First(), dut.Meeting.OptionalParticipantOids.First());
            Assert.AreEqual(_optionalParticipantEmails.First(), dut.Meeting.OptionalParticipantEmails.First());
        }
    }
}
