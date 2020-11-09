using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IBlobStorage> _blobStorageMock;
        private IOptionsMonitor<BlobStorageOptions> _monitorMock;
        private DeleteAttachmentCommandHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            _invitation = new Invitation(PLANT, "TestProject", "TestInvitation", "Description", DisciplineType.DP);
            var attachment = new TestableAttachment(PLANT, "ExistingFile.txt");
            attachment.SetId(2);
            _invitation.AddAttachment(attachment);

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

            _dut = new DeleteAttachmentCommandHandler(
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _blobStorageMock.Object,
                _monitorMock);
        }

        [TestMethod]
        public async Task DeletingAttachment_RemovesItFromInvitationAndBlobStorage()
        {
            var command = new DeleteAttachmentCommand(1, 2, "AAAAAAAAAAA=");
            var result = await _dut.Handle(command, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(0, _invitation.Attachments.Count);
            _blobStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), default), Times.Once);
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
