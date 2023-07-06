using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttachmentUploadedEvent : IDomainEvent
    {
        public AttachmentUploadedEvent(string plant, Guid sourceGuid, string fileName)
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
