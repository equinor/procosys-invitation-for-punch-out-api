using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetAttachmentById
{
    [TestClass]
    public class GetAttachmentsQueryHandlerTests : ReadOnlyTestsBase
    {
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
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectAttachment()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var blobStorageMock = new Mock<IBlobStorage>();
            var blobStorageOptions = new BlobStorageOptions();
            var query = new GetAttachmentByIdQuery(1, 2);

            var dut = new GetAttachmentByIdQueryHandler(context, blobStorageMock.Object, blobStorageOptions);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Data.Id);
        }

        [TestMethod]
        public async Task Handle_ReturnsNotFound_IfAttachmentIsNotFound()
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var blobStorageMock = new Mock<IBlobStorage>();
            var blobStorageOptions = new BlobStorageOptions();
            var query = new GetAttachmentByIdQuery(1, 3);

            var dut = new GetAttachmentByIdQueryHandler(context, blobStorageMock.Object, blobStorageOptions);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(ResultType.NotFound, result.ResultType);
            Assert.IsNull(result.Data);
        }
    }
}
