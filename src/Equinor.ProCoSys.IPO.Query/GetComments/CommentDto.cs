using System;

namespace Equinor.ProCoSys.IPO.Query.GetComments
{
    public class CommentDto
    {
        public CommentDto(
            int id,
            string comment,
            PersonDto createdBy,
            DateTime createdAtUtc,
            string rowVersion)
        {
            Id = id;
            Comment = comment;
            CreatedBy = createdBy;
            CreatedAtUtc = createdAtUtc;
            RowVersion = rowVersion;
        }
        public int Id { get; }
        public string Comment { get; }
        public PersonDto CreatedBy { get; }
        public DateTime CreatedAtUtc { get; }
        public string RowVersion { get; }
    }
}
