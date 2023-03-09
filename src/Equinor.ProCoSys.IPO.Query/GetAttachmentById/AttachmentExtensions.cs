using System;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.Common.Time;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public static class AttachmentExtensions
    {
        public static Uri GetAttachmentDownloadUri(this Attachment attachment, IAzureBlobService blobStorage, BlobStorageOptions blobStorageOptions)
        {
            var fullBlobPath = attachment.GetFullBlobPath();
            var now = TimeService.UtcNow;
            var uri = blobStorage.GetDownloadSasUri(
                blobStorageOptions.BlobContainer,
                fullBlobPath,
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes * -1)),
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes)));
            return uri;
        }
    }
}
