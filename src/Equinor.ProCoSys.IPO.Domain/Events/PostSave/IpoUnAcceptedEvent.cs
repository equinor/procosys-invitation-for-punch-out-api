using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoUnAcceptedEvent : IPostSaveDomainEvent
    {
        public IpoUnAcceptedEvent(
            string plant,
            Guid sourceGuid)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
