using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentAddedEvent : DomainEvent
    {
        public CommentAddedEvent(
            string plant,
            Guid sourceGuid) : base("Comment added")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
