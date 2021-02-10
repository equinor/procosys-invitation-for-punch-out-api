using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentUploadedEvent : INotification
    {
        public AttachmentUploadedEvent(
            string plant,
            Guid objectGuid,
            string fileName)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            FileName = fileName;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public string FileName { get; }
    }
}
