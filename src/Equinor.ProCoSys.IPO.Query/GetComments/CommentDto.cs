using System;

namespace Equinor.ProCoSys.IPO.Query.GetComments
{
    public class CommentDto
    {
        public CommentDto(
            int id,
            string comment,
            PersonDto createdBy,
            DateTime createdAtUtc)
        {
            Id = id;
            Comment = comment;
            CreatedBy = createdBy;
            CreatedAtUtc = createdAtUtc;
        }
        public int Id { get; }
        public string Comment { get; }
        public PersonDto CreatedBy { get; }
        public DateTime CreatedAtUtc { get; }
    }
}
