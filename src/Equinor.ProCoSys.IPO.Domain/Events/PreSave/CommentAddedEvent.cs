using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentAddedEvent : DomainEvent
    {
        public CommentAddedEvent(
            string plant,
            Guid objectGuid) : base("Comment added")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
