using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnAcceptedEvent : DomainEvent
    {
        public IpoUnAcceptedEvent(
            string plant,
            Guid sourceGuid) : base("IPO unaccepted")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
