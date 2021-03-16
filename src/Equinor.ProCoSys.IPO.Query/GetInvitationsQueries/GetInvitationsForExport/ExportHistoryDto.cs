using System;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class ExportHistoryDto
    {
        public ExportHistoryDto(
            int id,
            string description,
            DateTime createdAtUtc,
            string createdBy)
        {
            Id = id;
            Description = description;
            CreatedAtUtc = createdAtUtc;
            CreatedBy = createdBy;
        }

        public int Id { get; }
        public string Description { get; }
        public DateTime CreatedAtUtc { get; }
        public string CreatedBy { get; }
    }
}
