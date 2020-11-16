using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Query.GetAttachments;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetAttachments
{
    [TestClass]
    public class GetAttachmentsQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IOptionsMonitor<BlobStorageOptions>> blobStorageOptionsMonitorMock;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(TestPlant, "TestProject", "TestInvitation", "TestDescriptioN", DisciplineType.DP);
                var attachmentA = new Attachment(TestPlant, "fileA.txt");
                var attachmentB = new Attachment(TestPlant, "fileB.txt");
                invitation.AddAttachment(attachmentA);
                invitation.AddAttachment(attachmentB);
                context.Invitations.Add(invitation);
                context.SaveChangesAsync().Wait();
            }

            var blobStorageOptions = new BlobStorageOptions();
            blobStorageOptionsMonitorMock = new Mock<IOptionsMonitor<BlobStorageOptions>>();
            blobStorageOptionsMonitorMock
                .Setup(x => x.CurrentValue)
                .Returns(blobStorageOptions);
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectNumberOfAttachments()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var blobStorageMock = new Mock<IBlobStorage>();
            var query = new GetAttachmentsQuery(1);
            var dut = new GetAttachmentsQueryHandler(context, blobStorageMock.Object, blobStorageOptionsMonitorMock.Object);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectAttachmentDetails()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var blobStorageMock = new Mock<IBlobStorage>();
            var query = new GetAttachmentsQuery(1);
            var dut = new GetAttachmentsQueryHandler(context, blobStorageMock.Object, blobStorageOptionsMonitorMock.Object);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Data[0].Id);
            Assert.AreEqual("fileA.txt", result.Data[0].FileName);
            Assert.AreEqual(2, result.Data[1].Id);
            Assert.AreEqual("fileB.txt", result.Data[1].FileName);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_IfAttachmentIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var blobStorageMock = new Mock<IBlobStorage>();
            var query = new GetAttachmentByIdQuery(1, 3);
            var dut = new GetAttachmentByIdQueryHandler(context, blobStorageMock.Object, blobStorageOptionsMonitorMock.Object);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(ResultType.NotFound, result.ResultType);
            Assert.IsNull(result.Data);
        }
    }
}
