using System;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class ExportParticipantDto
    {
        public ExportParticipantDto(
            int id, 
            string organization,
            string type,
            string participant,
            bool attended,
            string note,
            DateTime? signedAtUtc,
            string signedBy,
            int? signedById)
        {
            Id = id;
            Organization = organization;
            Type = type;
            Participant = participant;
            Attended = attended;
            Note = note;
            SignedAtUtc = signedAtUtc;
            SignedBy = signedBy;
            SignedById = signedById;
        }

        public int Id { get; }
        public string Organization { get; }
        public string Type { get; }
        public string Participant { get; }
        public bool Attended { get; }
        public string Note { get; }
        public DateTime? SignedAtUtc { get; }
        public string SignedBy { get; set; }
        public int? SignedById { get; }
    }
}
