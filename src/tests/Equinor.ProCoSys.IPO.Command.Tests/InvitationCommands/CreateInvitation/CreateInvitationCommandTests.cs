using System;
using System.Collections.Generic;
using System.Linq;
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
                new FunctionalRoleForCommand("FR1", "fr@test.com", false, null),
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
        private readonly string _body = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;
        private readonly List<McPkgScopeForCommand> _mcPkgScope = new List<McPkgScopeForCommand>
        {
            new McPkgScopeForCommand("MC1", "MC description", "comm parent"),
            new McPkgScopeForCommand("MC2", "MC description 2", "comm parent")
        };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new CreateInvitationCommand(
                _title,
                _body,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type, 
                _participants, 
                _mcPkgScope,
                null);

            Assert.AreEqual(_participants, dut.Participants);
            Assert.AreEqual(_title, dut.Title);
            Assert.AreEqual(_body, dut.BodyHtml);
            Assert.AreEqual(_location, dut.Location);
            Assert.AreEqual(new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc), dut.StartTime);
            Assert.AreEqual(new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc), dut.EndTime);
            Assert.AreEqual(2, dut.Participants.Count());
            Assert.AreEqual(_participants.First(), dut.Participants.First());
        }
    }
}
