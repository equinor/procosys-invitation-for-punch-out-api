namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ExternalEmailDto
    {
        public ExternalEmailDto(
            int id,
            string externalEmail)
        {
            Id = id;
            ExternalEmail = externalEmail;
        }

        public int Id { get; }
        public string ExternalEmail { get; }
    }
}
