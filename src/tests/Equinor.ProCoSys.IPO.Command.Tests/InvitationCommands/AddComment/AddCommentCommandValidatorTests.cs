using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AddComment
{
    [TestClass]
    public class AddCommentCommandValidatorTests
    {
        private AddCommentCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;

        private AddCommentCommand _command;
        private const int _invitationId = 1;
        private string _commentText = "comment text";

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_invitationId, default)).Returns(Task.FromResult(true));
            _command = new AddCommentCommand(
                _invitationId,
                _commentText);

            _dut = new AddCommentCommandValidator(_invitationValidatorMock.Object);
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
        public async Task Validate_ShouldFail_WhenInvitationIsInCanceledStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_invitationId, IpoStatus.Canceled, default)).Returns(Task.FromResult(true));

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is canceled, and thus cannot be commented on"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCommentIsTooLong()
        {
            var longComment = new string('x', Comment.CommentMaxLength + 1);
            var result = _dut.Validate(new AddCommentCommand(1, longComment));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Comment cannot be more than {Comment.CommentMaxLength} characters!"));
        }
    }
}
