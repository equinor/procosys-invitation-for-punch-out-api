using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.DeleteAttachment
{
    [TestClass]
    public class DeleteAttachmentCommandValidatorTests
    {
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;
        private DeleteAttachmentCommandValidator _dut;

        [TestInitialize]
        public void Setup()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _invitationValidatorMock
                .Setup(x => x.IpoExistsAsync(0, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));
            _invitationValidatorMock
                .Setup(x => x.IpoExistsAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(x => x.AttachmentExistsAsync(1, 0, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));
            _invitationValidatorMock.Setup(x => x.AttachmentExistsAsync(1, 2, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock
                .Setup(x => x.IsValid("ABC"))
                .Returns(true);

            _dut = new DeleteAttachmentCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
        }

        [TestMethod]
        public async Task Validate_Succeeds()
        {
            var command = new DeleteAttachmentCommand(1, 2, "ABC");

            var result = await _dut.ValidateAsync(command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public async Task Validate_Fails_WhenInvitationDoesNotExist()
        {
            var command = new DeleteAttachmentCommand(0, 2, "ABC");

            var result = await _dut.ValidateAsync(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation with this ID does not exist"));
        }

        [TestMethod]
        public async Task Validate_Fails_WhenAttachmentDoesNotExist()
        {
            var command = new DeleteAttachmentCommand(1, 0, "ABC");

            var result = await _dut.ValidateAsync(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Attachment doesn't exist!"));
        }

        [TestMethod]
        public async Task Validate_Fails_WhenRowVersionIsInvalid()
        {
            var command = new DeleteAttachmentCommand(1, 2, "Invalid");

            var result = await _dut.ValidateAsync(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Not a valid row version!"));
        }
    }
}
