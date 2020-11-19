namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class ParticipantToUpdateNoteDto
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public string RowVersion { get; set; }
    }
}
