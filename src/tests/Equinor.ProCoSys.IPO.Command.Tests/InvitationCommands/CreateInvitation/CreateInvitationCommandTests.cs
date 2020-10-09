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
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var meeting = new CreateMeetingCommand(
                    "title",
                    "body",
                    "location",
                    new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                    new List<Guid> { new Guid("12345678-1234-1234-1234-123456123456") },
                    new List<string> { "abc@example.com" });
            var dut = new CreateInvitationCommand(meeting);

            Assert.AreEqual(meeting, dut.Meeting);
            Assert.AreEqual("title", dut.Meeting.Title);
            Assert.AreEqual("body", dut.Meeting.BodyHtml);
            Assert.AreEqual("location", dut.Meeting.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.Meeting.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.Meeting.EndTime);
            Assert.IsNotNull(dut.Meeting.ParticipantOids);
            Assert.AreEqual(1, dut.Meeting.ParticipantOids.Count());
            Assert.AreEqual("12345678-1234-1234-1234-123456123456", dut.Meeting.ParticipantOids.First().ToString());
            Assert.AreEqual("abc@example.com", dut.Meeting.ParticipantEmails.First().ToString());
        }
    }
}
