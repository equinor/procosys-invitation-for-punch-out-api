using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentUploadedEvent : DomainEvent
    {
        public AttachmentUploadedEvent(
            string plant,
            Guid sourceGuid,
            string fileName) : base("Attachment uploaded")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            FileName = fileName;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public string FileName { get; }
    }
}
