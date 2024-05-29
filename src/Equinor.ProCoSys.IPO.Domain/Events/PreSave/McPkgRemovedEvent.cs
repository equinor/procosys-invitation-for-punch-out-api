using Equinor.ProCoSys.Common;
using System;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class McPkgRemovedEvent : IDomainEvent
{
    public McPkgRemovedEvent(string plant, Guid sourceGuid, Guid mcPkgGuid)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        McPkgGuid = mcPkgGuid;
    }
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public Guid McPkgGuid { get; }
}
