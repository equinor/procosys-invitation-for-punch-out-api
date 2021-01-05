using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
        private const string PLANT = "PCS$TESTPLANT";

        private Invitation _invitation;
        private Attachment _attachment;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IBlobStorage> _blobStorageMock;
        private IOptionsMonitor<BlobStorageOptions> _monitorMock;
        private DeleteAttachmentCommandHandler _dut;
        private DeleteAttachmentCommand _command;

        [TestInitialize]
        public void Setup()
        {
            _invitation = new Invitation(
                PLANT,
                "TestProject",
                "TestInvitation", 
                "Description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null);
            _attachment = new TestableAttachment(PLANT, "ExistingFile.txt");
            _attachment.SetProtectedIdForTesting(2);
            _invitation.AddAttachment(_attachment);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .Returns(Task.FromResult(_invitation));

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _blobStorageMock = new Mock<IBlobStorage>();
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
            _blobStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), default), Times.Once);
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

        private class TestableAttachment : Attachment
        {
            public TestableAttachment(string plant, string fileName)
                : base(plant, fileName)
            {
            }
            public void SetId(int id) => Id = id;
        }
    }
}
