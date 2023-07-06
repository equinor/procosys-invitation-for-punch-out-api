using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class ScopeHandedOverEvent : DomainEvent
    {
        public ScopeHandedOverEvent(
            string plant,
            Guid sourceGuid) : base("Scope handed over")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
