using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.EditInvitation
{
    [TestClass]
    public class EditInvitationCommandValidatorTests
    {
        private EditInvitationCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private EditInvitationCommand _command;
        private const string _title = "Test title";
        private const string _description = "body";
        private const string _location = "location A";
        private const int _id = 1;
        private const DisciplineType _type = DisciplineType.DP;
        private const string _rowVersion = "AAAAAAAAABA=";

        private readonly IList<string> _commPkgScope = new List<string> {"COMM-02"};
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
                new PersonForCommand(null, "ola@test.com", true),
                null,
                1)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_rowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(new List<string>(), _commPkgScope)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Planned, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_participants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_participants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(true);
            _command = new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion);

            _dut = new EditInvitationCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("IPO with this ID does not exist!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantsIsNull()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                null,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participants cannot be null!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenDescriptionIsTooLong()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                _title,
                new string('x', Invitation.DescriptionMaxLength + 1),
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Description cannot be more than {Invitation.DescriptionMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenStartDateIsAfterEndDate()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Start time must be before end time!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsTooShort()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                "t",
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Title must be between 3 and 1024 characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsTooLong()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                new string('x', Invitation.TitleMaxLength + 1),
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Title must be between 3 and 1024 characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsNull()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                null,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Title must be between 3 and 1024 characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_LocationIsTooLong()
        {
            var result = _dut.Validate(new EditInvitationCommand(
                _id,
                _title,
                _description,
                new string('x', Invitation.LocationMaxLength + 1),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _participants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Location cannot be more than 1024 characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenScopeIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(new List<string>(), _commPkgScope)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Not a valid scope! Choose either mc scope or comm pkg scope"));
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

        [TestMethod]
        public void Validate_ShouldFail_WhenRequiredParticipantsDoNotHaveLowestSortKeys()
        {
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantsWithIdsDoNotExist()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_participants[0], _id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist"));
        }
    }
}
