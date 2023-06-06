using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentRemovedEvent : DomainEvent
    {
        public CommentRemovedEvent(
            string plant,
            Guid sourceGuid) : base("Comment removed")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
