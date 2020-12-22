using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public class AttachmentRemovedEvent : INotification
    {
        public AttachmentRemovedEvent(
            string plant,
            Guid objectGuid,
            int invitationId,
            string attachmentTitle)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            InvitationId = invitationId;
            AttachmentTitle = attachmentTitle;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public int InvitationId { get; }
        public string AttachmentTitle { get; }
    }
}
