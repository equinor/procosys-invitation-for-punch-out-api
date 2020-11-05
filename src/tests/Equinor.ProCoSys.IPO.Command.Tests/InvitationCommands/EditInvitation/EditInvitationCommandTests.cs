using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.EditInvitation
{
    [TestClass]
    public class EditInvitationCommandTests
    {
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand("FR1", null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(null, "Ola", "Nordman", "ola@test.com", true),
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
        private readonly string _rowVersion = "AAAAAAAAABA=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new EditInvitationCommand(
                1,
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
                _rowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(_title, dut.Title);
            Assert.AreEqual(_description, dut.Description);
            Assert.AreEqual(_location, dut.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.EndTime);
            Assert.IsNotNull(dut.UpdatedParticipants);
            Assert.AreEqual(2, dut.UpdatedParticipants.Count);
            Assert.IsNotNull(dut.UpdatedMcPkgScope);
            Assert.AreEqual(2, dut.UpdatedMcPkgScope.Count);
        }
    }
}
