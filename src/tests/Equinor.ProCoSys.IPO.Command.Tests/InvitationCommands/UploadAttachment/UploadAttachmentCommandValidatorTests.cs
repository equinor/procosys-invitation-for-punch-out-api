using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UploadAttachment
{
    [TestClass]
    public class UploadAttachmentCommandValidatorTests
    {
        private Mock<IInvitationValidator> _validatorMock;
        private IOptionsMonitor<BlobStorageOptions> _monitor;
        private UploadAttachmentCommandValidator _dut;

        [TestInitialize]
        public void Setup()
        {
            _validatorMock = new Mock<IInvitationValidator>();
            _validatorMock
                .Setup(x => x.IpoExistsAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _validatorMock.Setup(x => x.AttachmentWithFileNameExistsAsync(1, "existingfile.txt", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var blobStorageOptions = new BlobStorageOptions()
            {
                BlobContainer = "TestContainer",
                MaxSizeMb = 1,
                BlockedFileSuffixes = new[] { ".exe" }
            };
            _monitor = Mock.Of<IOptionsMonitor<BlobStorageOptions>>(x => x.CurrentValue == blobStorageOptions);
            _dut = new UploadAttachmentCommandValidator(_validatorMock.Object, _monitor);
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

        [TestMethod]
        public void Validate_Fails_WhenFileNameIsNotGiven()
        {

            var command = new UploadAttachmentCommand(1, string.Empty, false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Filename not given!"));
        }

        [TestMethod]
        public void Validate_Fails_WhenFileNameIsTooLong()
        {

            var command = new UploadAttachmentCommand(1, new string('a', 252) + ".txt", false, new MemoryStream());

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Filename too long!"));
        }

        [TestMethod]
        public void Validate_Fails_WhenAttachmentIsTooLarge()
        {
            var data = new byte[2 * 1024 * 1024];
            var command = new UploadAttachmentCommand(1, "newfile.txt", false, new MemoryStream(data));

            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Maximum file size is"));
        }
    }
}
