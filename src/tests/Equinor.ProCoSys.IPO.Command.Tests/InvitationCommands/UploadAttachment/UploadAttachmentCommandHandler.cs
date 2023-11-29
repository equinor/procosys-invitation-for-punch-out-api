using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UploadAttachment
{
    [TestClass]
    public class UploadAttachmentCommandHandlerTests
    {
        private const string _plant = "PCS$TESTPLANT";
        private const string _projectName = "TestProject";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName}", _projectGuid);

        private Invitation _invitation;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IAzureBlobService> _blobStorageMock;
        private IOptionsMonitor<BlobStorageOptions> _monitorMock;
        private UploadAttachmentCommandHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            _invitation = new Invitation(
                _plant,
                _project,
                "TestInvitation",
                "Description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(_plant, _project, "Comm", "Mc", "d", "1|2", Guid.Empty, Guid.Empty)},
                null);
            _invitation.AddAttachment(new Attachment(_plant, "ExistingFile.txt"));
            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .Returns(Task.FromResult(_invitation));
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);
            _blobStorageMock = new Mock<IAzureBlobService>();
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
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), false, default), Times.Once);
        }

        [TestMethod]
        public async Task AddingExistingFileToDatabase_WithoutOverwrite_Fails()
        {
            var command = new UploadAttachmentCommand(1, "ExistingFile.txt", false, new MemoryStream());
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Invalid, result.ResultType);
            Assert.AreEqual(1, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), default), Times.Never);
        }

        [TestMethod]
        public async Task AddingExistingFileToDatabase_WithOverwrite_Succeeds()
        {
            var command = new UploadAttachmentCommand(1, "ExistingFile.txt", true, new MemoryStream());
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(1, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), true, default), Times.Once);
        }
    }
}
