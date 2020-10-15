using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
        private readonly string _body = "body";
        private readonly string _location = "location A";
        private readonly DisciplineType _type = DisciplineType.DP;

        private readonly IList<McPkgScopeForCommand> _mcPkgScope = new List<McPkgScopeForCommand>
        {
            new McPkgScopeForCommand("MC01", "D1", "COMM-01")
        };
        private readonly IList<CommPkgScopeForCommand> _commPkgScope = new List<CommPkgScopeForCommand>
        {
            new CommPkgScopeForCommand("COMM-02", "D2", "PA")
        };
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
                new PersonForCommand(Guid.Empty, "Ola", "Nordman", "ola@test.com", true),
                null,
                1)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(new List<McPkgScopeForCommand>(), _commPkgScope)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(true);
            _command = new CreateInvitationCommand(
                _title,
                _body,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope);
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

        [TestMethod]
        public void Validate_ShouldFail_WhenScopeIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(new List<McPkgScopeForCommand>(), _commPkgScope)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Scope must be valid. Either mc scope or comm pgk scope must be added, but not both!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantListIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Each participant must contain an email"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantListDoesNotHaveRequiredParticipants()
        {
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor and Construction Company must be invited"));
        }
    }
}
