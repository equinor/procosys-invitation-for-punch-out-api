using Equinor.ProCoSys.Common;
using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class CommPkgAddedEvent : IDomainEvent
{
    public CommPkgAddedEvent(string plant, Guid sourceGuid, CommPkg commPkg, Invitation invitation)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        CommPkg = commPkg;
        Invitation = invitation;
    }
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public CommPkg CommPkg { get; }
    public Invitation Invitation { get; }
}
