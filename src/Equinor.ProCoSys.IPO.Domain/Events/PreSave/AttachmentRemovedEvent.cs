using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentRemovedEvent : INotification
    {
        public AttachmentRemovedEvent(
            string plant,
            Guid objectGuid,
            string attachmentTitle)
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
