using System;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public class AttachmentDto
    {
        public AttachmentDto(int id, string fileName, Uri downloadUri, DateTime uploadedAt, PersonDto uploadedBy, string rowVersion)
        {
            Id = id;
            FileName = fileName;
            DownloadUri = downloadUri;
            UploadedAt = uploadedAt;
            UploadedBy = uploadedBy;
            RowVersion = rowVersion;
        }

        public Uri DownloadUri { get; }
        public string FileName { get; }
        public int Id { get; }
        public string RowVersion { get; }
        public DateTime UploadedAt { get; }
        public PersonDto UploadedBy { get; }
    }
}
