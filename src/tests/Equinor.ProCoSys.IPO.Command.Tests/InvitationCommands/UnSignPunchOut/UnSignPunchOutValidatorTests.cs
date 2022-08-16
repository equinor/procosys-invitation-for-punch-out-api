using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UnSignPunchOut;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UnSignPunchOut
{
    [TestClass]
    public class UnSignPunchOutCommandValidatorTests
    {
        private UnSignPunchOutCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private UnSignPunchOutCommand _command;
        private const int _invitationId = 1;
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private const int _participantId = 10;

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_invitationId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationId, _participantId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.SignerExistsAsync(_invitationId, _participantId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId, _invitationId, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantIsSignedAsync(_participantId, _invitationId, default)).Returns(Task.FromResult(true));

            _command = new UnSignPunchOutCommand(
                _invitationId,
                _participantId,
                _participantRowVersion);

            _dut = new UnSignPunchOutCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_invitationId, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation with this ID does not exist!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId, _invitationId, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist on invitation!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationIsInCanceledStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_invitationId, IpoStatus.Canceled, default)).Returns(Task.FromResult(true));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is canceled, and thus cannot be unsigned"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant row version is not valid!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationDoesNotHaveSigningParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.SignerExistsAsync(_invitationId, _participantId, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant is not assigned to unsign this IPO"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantIsNotSigned()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantIsSignedAsync(_participantId, _invitationId, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("An unsigned participant cannot be unsigned"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenPersonTryingToCompleteIsNotAValidSigningParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationId, _participantId, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("Person unsigning is not admin, not assigned to unsign IPO, or there is not a valid functional role on the IPO!"));
        }
    }
}
