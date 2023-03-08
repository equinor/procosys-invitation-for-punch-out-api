using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.Common.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Comment : PlantEntityBase, ICreationAuditable
    {
        public const int CommentMaxLength = 4096;

        protected Comment()
            : base(null)
        {
        }

        public Comment(string plant, string comment)
            : base(plant)
        {
            if (string.IsNullOrEmpty(comment))
            {
                throw new ArgumentNullException(nameof(comment));
            }
 
            CommentText = comment;
        }

        public string CommentText { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }
    }
}
