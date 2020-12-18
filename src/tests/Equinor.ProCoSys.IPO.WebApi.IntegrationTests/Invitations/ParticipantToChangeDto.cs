namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class ParticipantToChangeDto
    {
        public int Id { get; set; }
        public bool Attended { get; set; }
        public string Note { get; set; }
        public string RowVersion { get; set; }
    }
}
