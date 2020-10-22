using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UploadAttachment
{
    [TestClass]
    public class UploadAttachmentCommandValidatorTests
    {
        private Mock<IInvitationValidator> _validatorMock;
        private UploadAttachementCommandValidator _dut;

        [TestInitialize]
        public void Setup()
        {
            _validatorMock = new Mock<IInvitationValidator>();
            _validatorMock
                .Setup(x => x.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _validatorMock.Setup(x => x.AttachmentWithFileNameExistsAsync(1, "existingfile.txt", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _dut = new UploadAttachementCommandValidator(_validatorMock.Object);
        }

        [TestMethod]
        public void Validate_Succeeds_WhenInvitationExists()
        {
            var command = new UploadAttachmentCommand(1, "newfile.txt", false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_Fails_WhenInvitationDoesNotExist()
        {
            var command = new UploadAttachmentCommand(-1, "newfile.txt", false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation doesn't exist!"));
        }

        [TestMethod]
        public void Validate_Fails_WhenAttachmentExists()
        {
            var command = new UploadAttachmentCommand(1, "existingfile.txt", false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation already has an attachment with filename"));
        }

        [TestMethod]
        public void Validate_Succeeds_WhenAttachmentDoesNotExist()
        {
            var command = new UploadAttachmentCommand(1, "newfile.txt", false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }
    }
}
