namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class ExportParticipantDto
    {
        public ExportParticipantDto(
            int id, 
            string organization,
            string type,
            string participant)
        {
            Id = id;
            Organization = organization;
            Type = type;
            Participant = participant;
        }

        public int Id { get; }
        public string Organization { get; }
        public string Type { get; }
        public string Participant { get; }
    }
}
