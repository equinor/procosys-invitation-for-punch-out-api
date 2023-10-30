using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.DeleteAttachment
{
    [TestClass]
    public class DeleteAttachmentCommandHandlerTests
    {
        private const string _plant = "PCS$TESTPLANT";
        private const string _projectName = "TestProject";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName}", _projectGuid);

        private Invitation _invitation;
        private Attachment _attachment;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IAzureBlobService> _blobStorageMock;
        private IOptionsMonitor<BlobStorageOptions> _monitorMock;
        private DeleteAttachmentCommandHandler _dut;
        private DeleteAttachmentCommand _command;

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
                new List<McPkg> { new McPkg(_plant, _project, "Comm", "Mc", "d", "1|2")},
                null);
            _attachment = new Attachment(_plant, "ExistingFile.txt");
            _attachment.SetProtectedIdForTesting(2);
            _invitation.AddAttachment(_attachment);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .Returns(Task.FromResult(_invitation));

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _blobStorageMock = new Mock<IAzureBlobService>();
            var blobStorageOptions = new BlobStorageOptions()
            {
                BlobContainer = "TestContainer"
            };
            _monitorMock = Mock.Of<IOptionsMonitor<BlobStorageOptions>>(x => x.CurrentValue == blobStorageOptions);

            _command = new DeleteAttachmentCommand(1, 2, "AAAAAAAAAAA=");

            _dut = new DeleteAttachmentCommandHandler(
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _blobStorageMock.Object,
                _monitorMock);
        }

        [TestMethod]
        public async Task DeletingAttachment_RemovesAttachmentFromBlobStorage()
        {
            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            _blobStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        }

        [TestMethod]
        public async Task DeletingAttachment_RemovesAttachmentFromInvitation()
        {
            Assert.AreEqual(1, _invitation.Attachments.Count);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(0, _invitation.Attachments.Count);
        }

        [TestMethod]
        public async Task DeletingAttachment_RemovesAttachmentFromAttachmentTable()
        {
            Assert.AreEqual(1, _invitation.Attachments.Count);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(0, _invitation.Attachments.Count);
            _invitationRepositoryMock.Verify(r => r.RemoveAttachment(_attachment), Times.Once);
        }

        [TestMethod]
        public async Task DeletingAttachment_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}
