using System;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public static class AttachmentExtensions
    {
        public static Uri GetAttachmentDownloadUri(this Attachment attachment, IBlobStorage blobStorage, BlobStorageOptions blobStorageOptions)
        {
            var fullBlobPath = string.Join('/', blobStorageOptions.BlobContainer, attachment.BlobPath);

            var now = TimeService.UtcNow;
            var uri = blobStorage.GetDownloadSasUri(
                fullBlobPath,
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes * -1)),
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes)));
            return uri;
        }
    }
}
