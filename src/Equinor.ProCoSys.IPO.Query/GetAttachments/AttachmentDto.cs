namespace Equinor.ProCoSys.IPO.Query.GetAttachments
{
    public class AttachmentDto
    {
        public AttachmentDto(int id, string fileName, string rowVersion)
        {
            Id = id;
            FileName = fileName;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string FileName { get; }
        public string RowVersion { get; }
    }
}
