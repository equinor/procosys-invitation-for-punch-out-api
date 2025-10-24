using System;
using Azure.Storage.Blobs.Models;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
            // Arrange
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc)));
            var attachment = new Attachment("PCS$TESTPLANT", "testfile.txt");
            
            var blobStorageMock = new Mock<IAzureBlobService>();
            blobStorageMock.Setup(x => x.GetDownloadSasUri(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<UserDelegationKey>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                    ))
                .Returns((string container, string path, DateTimeOffset start, DateTimeOffset end, UserDelegationKey _, string _, string _) => new Uri($"{container}/{path}?{start.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}&{end.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}"));
            
            var blobStorageOptions = new BlobStorageOptions
            {
                BlobClockSkewMinutes = 5,
                BlobContainer = @"https://blobcontainer",
                BlockedFileSuffixes = new[] { ".txt" },
                MaxSizeMb = 1
            };
            
            var userDelegationProviderMock = new Mock<IQueryUserDelegationProvider>();
            userDelegationProviderMock.Setup(u => u.GetUserDelegationKey()).Returns(new Mock<UserDelegationKey>().Object);

            // Act
            var uri = attachment.GetAttachmentDownloadUri(blobStorageMock.Object, blobStorageOptions, userDelegationProviderMock.Object);

            // Assert
            var expected = $"{blobStorageOptions.BlobContainer}/{attachment.GetFullBlobPath()}?2020-01-01T11:55:00Z&2020-01-01T12:05:00Z";
            Assert.AreEqual(expected, uri.ToString());
        }
    }
}
