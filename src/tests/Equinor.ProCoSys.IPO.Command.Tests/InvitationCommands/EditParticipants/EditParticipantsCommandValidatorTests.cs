using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditParticipants;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.EditParticipants
{
    [TestClass]
    public class EditParticipantsCommandValidatorTests
    {
        private EditParticipantsCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private EditParticipantsCommand _command;
        private const int _id = 1;
        private const string _rowVersion = "AAAAAAAAABA=";

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
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.SignedParticipantsCannotBeAlteredAsync(_editParticipants, _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SortKeyCannotBeChangedForSignedFirstSignersAsync(_editParticipants, _id, default)).Returns(Task.FromResult(true));
            _command = new EditParticipantsCommand(
                _id,
                _editParticipants);

            _dut = new EditParticipantsCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation with this ID does not exist!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantsIsNull()
        {
            var result = _dut.Validate(new EditParticipantsCommand(
                _id,
                null));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participants cannot be null!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantListIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Each participant must contain an oid"));
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
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor must be first and Construction Company must be second"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantsWithIdsDoNotExist()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(_editParticipants[0], _id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipant_HasNegativeSortKey()
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
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[0], _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[1], _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[2], _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.SignedParticipantsCannotBeAlteredAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SortKeyCannotBeChangedForSignedFirstSignersAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new EditParticipantsCommand(
                _id,
                editParticipants);


            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key for participant must be a non negative number!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenChangingFirstContractorWhenCompleted()
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
                        1)
                };

            var participants = editParticipants.Cast<ParticipantsForCommand>().ToList();
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAdmin()).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants))
                .Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[0], _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[1], _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SignedParticipantsCannotBeAlteredAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SortKeyCannotBeChangedForSignedFirstSignersAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(false));
            var command = new EditParticipantsCommand(
                _id,
                editParticipants);

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Cannot change first contractor or construction company if they have signed!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TryingToEditSignedParticipant()
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
                    1)
            };

            var participants = editParticipants.Cast<ParticipantsForCommand>().ToList();
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[0], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantWithIdExistsAsync(editParticipants[1], _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock
                .Setup(inv => inv.SignedParticipantsCannotBeAlteredAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(false));
            var command = new EditParticipantsCommand(
                _id,
                editParticipants);

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("Participants that have signed must be unsigned before edited!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenFunctionalRoleIsInvalid()
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
            _invitationValidatorMock.Setup(inv => inv.SignedParticipantsCannotBeAlteredAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SortKeyCannotBeChangedForSignedFirstSignersAsync(editParticipants, _id, default))
                .Returns(Task.FromResult(true));
            var command = new EditParticipantsCommand(
                _id,
                editParticipants);

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters!"));
        }
    }
}
