using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class ScopeHandedOverEvent : IDomainEvent
    {
        public ScopeHandedOverEvent(
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
