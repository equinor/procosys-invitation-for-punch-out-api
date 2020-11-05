using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UploadAttachment
{
    [TestClass]
    public class UploadAttachmentCommandHandlerTests
    {
        private const string PLANT = "PCS$TESTPLANT";

        private Invitation _invitation;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IBlobStorage> _blobStorageMock;
        private IOptionsMonitor<BlobStorageOptions> _monitorMock;
        private UploadAttachmentCommandHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            _invitation = new Invitation(PLANT, "TestProject", "TestInvitation","Description", DisciplineType.DP);
            _invitation.AddAttachment(new Attachment(PLANT, "ExistingFile.txt"));
            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .Returns(Task.FromResult(_invitation));
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(PLANT);
            _blobStorageMock = new Mock<IBlobStorage>();
            var blobStorageOptions = new BlobStorageOptions()
            {
                BlobContainer = "TestContainer"
            };
            _monitorMock = Mock.Of<IOptionsMonitor<BlobStorageOptions>>(x => x.CurrentValue == blobStorageOptions);

            _dut = new UploadAttachmentCommandHandler(
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _plantProviderMock.Object,
                _blobStorageMock.Object,
                _monitorMock);
        }

        [TestMethod]
        public async Task AddingNewFileToDatabase_Succeeds()
        {
            var command = new UploadAttachmentCommand(1, "NewFile.txt", false, new MemoryStream());
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(2, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), false, default), Times.Once);
        }

        [TestMethod]
        public async Task AddingExistingFileToDatabase_WithoutOverwrite_Fails()
        {
            var command = new UploadAttachmentCommand(1, "ExistingFile.txt", false, new MemoryStream());
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Invalid, result.ResultType);
            Assert.AreEqual(1, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), default), Times.Never);
        }

        [TestMethod]
        public async Task AddingExistingFileToDatabase_WithOverwrite_Succeeds()
        {
            var command = new UploadAttachmentCommand(1, "ExistingFile.txt", true, new MemoryStream());
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(1, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), true, default), Times.Once);
        }
    }
}
