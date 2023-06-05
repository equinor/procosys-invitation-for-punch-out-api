using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentRemovedEvent : DomainEvent
    {
        public AttachmentRemovedEvent(
            string plant,
            Guid objectGuid,
            string attachmentTitle) : base("Attachment removed")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            AttachmentTitle = attachmentTitle;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public string AttachmentTitle { get; }
    }
}
