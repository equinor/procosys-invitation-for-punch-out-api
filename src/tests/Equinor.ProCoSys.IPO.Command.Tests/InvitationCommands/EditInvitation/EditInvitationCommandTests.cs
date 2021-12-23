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
        private readonly List<EditParticipantsForCommand> _participants = new List<EditParticipantsForCommand>
        {
            new EditParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new EditFunctionalRoleForCommand("FR1", null),
                0),
            new EditParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new EditPersonForCommand(null, "ola@test.com", true),
                null,
                1)
        };

        private const string Title = "Test title";
        private const string Description = "Body";
        private const string Location = "Outside";
        private const DisciplineType Type = DisciplineType.DP;
        private readonly List<string> _mcPkgScope = new List<string>
        {
            "MC1",
            "MC2"
        };
        private const string RowVersion = "AAAAAAAAABA=";
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new EditInvitationCommand(
                1,
                Title,
                Description,
                Location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                Type,
                _participants,
                _mcPkgScope,
                null,
                RowVersion);

            Assert.AreEqual(1, dut.InvitationId);
            Assert.AreEqual(Title, dut.Title);
            Assert.AreEqual(Description, dut.Description);
            Assert.AreEqual(Location, dut.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.EndTime);
            Assert.IsNotNull(dut.UpdatedParticipants);
            Assert.AreEqual(2, dut.UpdatedParticipants.Count);
            Assert.IsNotNull(dut.UpdatedMcPkgScope);
            Assert.AreEqual(2, dut.UpdatedMcPkgScope.Count);
        }
    }
}
