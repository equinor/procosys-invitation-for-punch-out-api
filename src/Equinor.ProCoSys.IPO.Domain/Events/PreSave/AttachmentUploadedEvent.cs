using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentUploadedEvent : DomainEvent
    {
        public AttachmentUploadedEvent(
            string plant,
            Guid objectGuid,
            string fileName) : base("Attachment uploaded")
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
