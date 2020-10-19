namespace Equinor.ProCoSys.IPO.BlobStorage
{
    public class BlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public int MaxSizeMb { get; set; }
        public string BlobContainer { get; set; }
        public int BlobClockSkewMinutes { get; set; }
        public string[] ValidFileSuffixes { get; set; }
    }
}
