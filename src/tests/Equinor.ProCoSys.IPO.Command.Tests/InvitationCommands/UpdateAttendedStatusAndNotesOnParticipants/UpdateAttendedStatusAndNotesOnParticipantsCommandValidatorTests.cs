using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    [TestClass]
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandValidatorTests
    {
        private UpdateAttendedStatusAndNotesOnParticipantsCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private UpdateAttendedStatusAndNotesOnParticipantsCommand _command;
        private const string _note = "note A";
        private const int _id = 1;
        private const int _participantId1 = 10;
        private const int _participantId2 = 20;
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<UpdateAttendedStatusAndNotesOnParticipantsForCommand> _participants = new List<UpdateAttendedStatusAndNotesOnParticipantsForCommand>
        {
            new UpdateAttendedStatusAndNotesOnParticipantsForCommand(
                _participantId1,
                true,
                _note,
                _participantRowVersion1),
            new UpdateAttendedStatusAndNotesOnParticipantsForCommand(
                _participantId2,
                true,
                _note,
                _participantRowVersion2)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion2)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ValidContractorParticipantExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId1, _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId2, _id, default)).Returns(Task.FromResult(true));
            _command = new UpdateAttendedStatusAndNotesOnParticipantsCommand(
                _id,
                _participants);

            _dut = new UpdateAttendedStatusAndNotesOnParticipantsCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
        public void Validate_ShouldFail_WhenParticipantIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId1, _id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist on invitation"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationIsNotInCompletedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is not in completed stage, and thus cannot change attended statuses"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(false);
            
            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant doesn't have valid rowVersion"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant doesn't have valid rowVersion"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationDoesNotHaveContractorParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The IPO does not have a contractor assigned to the IPO!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenPersonTryingToCompleteIsNotAValidContractorParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ValidContractorParticipantExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("User is not the contractor assigned to complete this IPO, or there is not a valid functional role on the IPO!"));
        }
    }
}
