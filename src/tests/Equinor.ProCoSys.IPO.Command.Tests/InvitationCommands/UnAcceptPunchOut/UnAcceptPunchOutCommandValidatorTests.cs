using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UnAcceptPunchOut
{
    [TestClass]
    public class UnAcceptPunchOutCommandValidatorTests
    {
        private UnAcceptPunchOutCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private UnAcceptPunchOutCommand _command;
        private const int _id = 1;
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Accepted, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAdminOrValidAccepterParticipantAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoHasAccepterAsync(_id, default)).Returns(Task.FromResult(true));
            _command = new UnAcceptPunchOutCommand(
                _id,
                _invitationRowVersion,
                _participantRowVersion);

            _dut = new UnAcceptPunchOutCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
        public async Task Validate_ShouldFail_WhenInvitationIsNotInAcceptedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Accepted, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is not in accepted stage, and thus cannot be unaccepted"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(false);
            
            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation row version is not valid!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationDoesNotHaveConstructionCompanyParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoHasAccepterAsync(_id, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The IPO does not have a construction company assigned to accept the IPO!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenPersonTryingToUnAcceptIsNotThePersonWhoAcceptedTheIpoOrAnAdmin()
        {
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAdminOrValidAccepterParticipantAsync(_id, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("Person trying to unaccept is not an admin and not the person who accepted the IPO!"));
        }
    }
}
