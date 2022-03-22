namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class ParticipantToUpdateAttendedStatusDto
    {
        public int Id { get; set; }
        public bool Attended { get; set; }
        public string RowVersion { get; set; }
    }
}
