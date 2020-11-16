using System;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetAttachmentById
{
    [TestClass]
    public class AttachmentExtensionsTests
    {

        [TestMethod]
        public void GetAttachmentDownloadUri_ReturnCorrectUri()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc)));
            var attachment = new Attachment("PCS$TESTPLANT", "testfile.txt");
            var blobStorageMock = new Mock<IBlobStorage>();
            blobStorageMock.Setup(x => x.GetDownloadSasUri(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .Returns((string path, DateTimeOffset start, DateTimeOffset end) => new Uri($"{path}?{start}&{end}"));
            var blobStorageOptions = new BlobStorageOptions
            {
                BlobClockSkewMinutes = 5,
                BlobContainer = @"https://blobcontainer",
                BlockedFileSuffixes = new string[] { ".txt" },
                ConnectionString = "connectionstring",
                MaxSizeMb = 1
            };

            var uri = attachment.GetAttachmentDownloadUri(blobStorageMock.Object, blobStorageOptions);

            Assert.AreEqual($"https://blobcontainer/{attachment.BlobPath}?01.01.2020 11:55:00 +00:00&01.01.2020 12:05:00 +00:00", uri.ToString());
        }
    }
}
