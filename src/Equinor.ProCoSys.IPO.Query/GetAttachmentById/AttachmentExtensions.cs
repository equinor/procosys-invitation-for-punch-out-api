using System;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public static class AttachmentExtensions
    {
        public static Uri GetAttachmentDownloadUri(this Attachment attachment, IAzureBlobService blobStorage, BlobStorageOptions blobStorageOptions, IQueryUserDelegationProvider queryUserDelegationProvider)
        {
            var fullBlobPath = attachment.GetFullBlobPath();
            var now = TimeService.UtcNow;
            var uri = blobStorage.GetDownloadSasUri(
                blobStorageOptions.BlobContainer,
                fullBlobPath,
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes * -1)),
                new DateTimeOffset(now.AddMinutes(blobStorageOptions.BlobClockSkewMinutes)),
                queryUserDelegationProvider.GetUserDelegationKey());
            return uri;
        }
    }
}
