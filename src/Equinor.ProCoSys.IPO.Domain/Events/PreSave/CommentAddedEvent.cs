using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentAddedEvent : IDomainEvent
    {
        public CommentAddedEvent(string plant, Guid sourceGuid, Invitation invitation, Comment comment)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Invitation = invitation;
            Comment = comment;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Invitation Invitation { get; }
        public Comment Comment { get; }
    }
}
