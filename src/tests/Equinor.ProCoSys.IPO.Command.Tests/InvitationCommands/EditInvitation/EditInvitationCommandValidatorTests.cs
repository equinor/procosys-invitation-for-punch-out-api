using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly IList<string> _commPkgScope = new List<string> { "COMM-02" };
        private readonly List<ParticipantsForEditCommand> _editParticipants = new List<ParticipantsForEditCommand>
        {
            new ParticipantsForEditCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForEditCommand(1, "FR1", null, _rowVersion),
                0),
            new ParticipantsForEditCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForEditCommand(2, new Guid(), true, _rowVersion),
                null,
                1)
        };
        private List<ParticipantsForCommand> _participants;

        [TestInitialize]
        public void Setup_OkState()
        {
            _participants = _editParticipants.Cast<ParticipantsForCommand>().ToList();

            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_rowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(_type, new List<string>(), _commPkgScope)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Planned, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(true);
            _command = new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion);

            _dut = new EditInvitationCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
        }

        [TestMethod]
        public async Task Validate_ShouldBeValid_WhenOkState()
        {
            var result = await _dut.ValidateAsync(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation with this ID does not exist!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantsIsNull()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
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
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participants must be invited!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenDescriptionIsTooLong()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                _title,
                new string('x', Invitation.DescriptionMaxLength + 1),
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Description cannot be more than {Invitation.DescriptionMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenStartDateIsAfterEndDate()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Start time must be before end time!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_TitleIsTooShort()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                "t",
                description: _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_TitleIsTooLong()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                new string('x', Invitation.TitleMaxLength + 1),
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_TitleIsNull()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                null,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_LocationIsTooLong()
        {
            var result = await _dut.ValidateAsync(new EditInvitationCommand(
                _id,
                _title,
                _description,
                new string('x', Invitation.LocationMaxLength + 1),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                _editParticipants,
                null,
                _commPkgScope,
                _rowVersion));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Location cannot be more than {Invitation.LocationMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenScopeIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(_type, new List<string>(), _commPkgScope)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Not a valid scope! Choose either DP with mc scope or MDP with comm pkg scope"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantListIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Each participant must contain an oid"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantListDoesNotHaveRequiredParticipants()
        {
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor and Construction Company must be invited"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenRequiredParticipantsDoNotHaveLowestSortKeys()
        {
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor must be first and Construction Company must be second"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantsWithIdsDoNotExist()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[0], _id, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipant_HasNegativeSortKey()
        {
            var editParticipants = new List<ParticipantsForEditCommand>
                {
                    new ParticipantsForEditCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(1, "FR1", null, _rowVersion),
                        0),
                    new ParticipantsForEditCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForEditCommand(2, new Guid(), true, _rowVersion),
                        null,
                        1),
                    new ParticipantsForEditCommand(
                        Organization.External,
                        new InvitedExternalEmailForEditCommand(null, "jon@test.no", null),
                        null,
                        null,
                        -3)
                };

            var participants = editParticipants.Cast<ParticipantsForCommand>().ToList();
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[2], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                editParticipants,
                null,
                _commPkgScope,
                _rowVersion);


            var result = await _dut.ValidateAsync(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key for participant must be a non negative number!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenFunctinalRoleIsInvalid()
        {
            var editParticipants = new List<ParticipantsForEditCommand>
                {
                    new ParticipantsForEditCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(1, "F", null, _rowVersion),
                        0),
                    new ParticipantsForEditCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForEditCommand(2, new Guid(), true, _rowVersion),
                        null,
                        1),
                    new ParticipantsForEditCommand(
                        Organization.External,
                        new InvitedExternalEmailForEditCommand(null, "jon@test.no", null),
                        null,
                        null,
                        -3)
                };

            var participants = editParticipants.Cast<ParticipantsForCommand>().ToList();
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[2], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new EditInvitationCommand(
                _id,
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _type,
                editParticipants,
                null,
                _commPkgScope,
                _rowVersion);


            var result = await _dut.ValidateAsync(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters!"));
        }
    }
}
