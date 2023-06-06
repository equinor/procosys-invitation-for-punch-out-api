using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCompletedEvent : DomainEvent
    {
        public IpoCompletedEvent(
            string plant,
            Guid sourceGuid) : base("IPO completed")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
