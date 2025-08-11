using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandTests
    {
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForCreateCommand("FR1", null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForCreateCommand(new Guid(), true),
                null,
                1)
        };

        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _description = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;
        private readonly List<string> _mcPkgScope = new List<string>
        {
            "MC1",
            "MC2"
        };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                _mcPkgScope,
                null,
                true);

            Assert.AreEqual(_participants, dut.Participants);
            Assert.AreEqual(_title, dut.Title);
            Assert.AreEqual(_description, dut.Description);
            Assert.AreEqual(_location, dut.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.EndTime);
            Assert.AreEqual(2, dut.Participants.Count());
            Assert.AreEqual(_participants.First(), dut.Participants.First());
            Assert.IsTrue(dut.IsOnline);
        }
    }
}
