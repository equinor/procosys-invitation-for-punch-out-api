using Equinor.ProCoSys.Common;
using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class McPkgAddedEvent : IDomainEvent
{
    public McPkgAddedEvent(string plant, Guid sourceGuid, McPkg mcPkg, Invitation invitation)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        McPkg = mcPkg;
        Invitation = invitation;
    }
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public McPkg McPkg { get; }
    public Invitation Invitation { get; }
}
