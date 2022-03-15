using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusOnParticipant;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateAttendedStatusOnParticipant
{
    [TestClass]
    public class UpdateAttendedStatusOnParticipantCommandValidatorTests
    {
        private UpdateAttendedStatusOnParticipantCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private UpdateAttendedStatusOnParticipantCommand _command;
        private const bool _attended = true;
        private const int _invitationId = 1;
        private const int _participantId1 = 10;
        private const string _participantRowVersion1 = "AAAAAAAAABB=";

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_invitationId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId1, _invitationId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.HasPermissionToEditParticipantAsync(_participantId1, _invitationId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.HasOppositeAttendedStatusAsync(_participantId1, _invitationId, _attended, default)).Returns(Task.FromResult(true));
            _command = new UpdateAttendedStatusOnParticipantCommand(_invitationId, _participantId1, _attended, _participantRowVersion1);
            _dut = new UpdateAttendedStatusOnParticipantCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_invitationId, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation with this ID does not exist!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId1, _invitationId, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist on invitation"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantDoesNotHavePermissionToEdit()
        {
            _invitationValidatorMock.Setup(inv => inv.HasPermissionToEditParticipantAsync(_participantId1, _invitationId, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The current user does not have sufficient privileges to edit this participant."));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCurrentAttendedStatusIsTheSameAsStatusInRequest()
        {
            _invitationValidatorMock.Setup(inv => inv.HasOppositeAttendedStatusAsync(_participantId1, _invitationId, _attended, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Cannot update participant to its current attendedStatus."));
        }
        
        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationIsCancelled()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_invitationId, IpoStatus.Canceled, default)).Returns(Task.FromResult(true));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Cannot perform updates on cancelled invitation"));
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
    }
}
