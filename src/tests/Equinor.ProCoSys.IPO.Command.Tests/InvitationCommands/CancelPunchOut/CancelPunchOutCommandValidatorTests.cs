using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CancelPunchOut
{
    [TestClass]
    public class CancelPunchOutCommandValidatorTests
    {
        private CancelPunchOutCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private CancelPunchOutCommand _command;
        private const int _id = 1;
        private const string _invitationRowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAllowedToCancelIpoAsync(_id, default)).Returns(Task.FromResult(true));
            _command = new CancelPunchOutCommand(_id, _invitationRowVersion);

            _dut = new CancelPunchOutCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
        public async Task Validate_ShouldFail_WhenInvitationIsInAcceptedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Accepted, default)).Returns(Task.FromResult(true));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("IPO is in accepted stage"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationIsInCanceledStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Canceled, default)).Returns(Task.FromResult(true));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("IPO is already canceled"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenInvitationRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(false);
            
            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation does not have valid rowVersion"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenUserTryingToCancelIsNotOrganizerOfIpo()
        {
            _invitationValidatorMock.Setup(inv => inv.CurrentUserIsAllowedToCancelIpoAsync(_id, default)).Returns(Task.FromResult(false));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Current user is not the creator of the invitation and not in Contractor Functional Role!"));
        }
    }
}
