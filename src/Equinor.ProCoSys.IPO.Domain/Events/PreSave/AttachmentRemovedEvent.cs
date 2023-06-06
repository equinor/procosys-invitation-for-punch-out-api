using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentRemovedEvent : DomainEvent
    {
        public AttachmentRemovedEvent(
            string plant,
            Guid sourceGuid,
            string attachmentTitle) : base("Attachment removed")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            AttachmentTitle = attachmentTitle;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public string AttachmentTitle { get; }
    }
}
