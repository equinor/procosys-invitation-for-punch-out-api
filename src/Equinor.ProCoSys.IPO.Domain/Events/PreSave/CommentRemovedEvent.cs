using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentRemovedEvent : DomainEvent
    {
        public CommentRemovedEvent(
            string plant,
            Guid objectGuid) : base("Comment removed")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
