using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCreatedEvent : DomainEvent
    {
        public IpoCreatedEvent(
            string plant,
            Guid sourceGuid) : base("IPO created")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
