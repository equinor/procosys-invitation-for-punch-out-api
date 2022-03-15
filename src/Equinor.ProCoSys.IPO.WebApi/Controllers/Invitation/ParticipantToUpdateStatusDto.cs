namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class ParticipantToUpdateStatusDto
    {
        public int Id { get; set; }
        public bool Attended { get; set; }
        public string RowVersion { get; set; }
    }
}
