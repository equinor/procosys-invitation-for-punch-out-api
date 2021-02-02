using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query
{
    public class ExternalEmailDto
    {
        public ExternalEmailDto(
            int id,
            string externalEmail,
            string rowVersion)
        {
            Id = id;
            ExternalEmail = externalEmail;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string ExternalEmail { get; }
        public OutlookResponse? Response { get; set; }
        public string RowVersion { get; }
    }
}
