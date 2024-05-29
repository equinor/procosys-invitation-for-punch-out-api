using Equinor.ProCoSys.Common;
using System;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class CommPkgAddedEvent : IDomainEvent
{
    public CommPkgAddedEvent(string plant, Guid sourceGuid, Guid commPkgGuid)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        CommPkgGuid = commPkgGuid;
    }
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public Guid CommPkgGuid { get; }
}
