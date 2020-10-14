using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandValidatorTests
    {
        private CreateInvitationCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private CreateInvitationCommand _command;
        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _type = "DP";

        private readonly IList<McPkgScopeForCommand> _mcPkgs = new List<McPkgScopeForCommand>
        {
            new McPkgScopeForCommand("MC01", "D1", "COMM-01")
        };
        private readonly IList<CommPkgScopeForCommand> _commPkgs = new List<CommPkgScopeForCommand>
        {
            new CommPkgScopeForCommand("COMM-02", "D2", "PA")
        };
        private readonly CreateMeetingCommand _meeting = new CreateMeetingCommand(
            "title",
            "body",
            "location",
            new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
            new List<Guid>() { new Guid("22222222-3333-3333-3333-444444444444") },
            new List<string>() { "abc@example.com" },
            new List<Guid>() { new Guid("33333333-4444-4444-4444-555555555555") },
            new List<string>() { "def@example.com" });

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();

            _command = new CreateInvitationCommand(_title, _projectName, _type, _meeting, _mcPkgs, null);
            _dut = new CreateInvitationCommandValidator(_invitationValidatorMock.Object);
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationWithSameTitleExistsInProject()
        {
            _invitationValidatorMock.Setup(inv => inv.TitleExistsOnProjectAsync(_projectName, _title, default)).Returns(Task.FromResult(true));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("IPO with this title already exists in project!"));
        }
    }
}
