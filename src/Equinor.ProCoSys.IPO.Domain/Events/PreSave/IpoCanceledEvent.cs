using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCanceledEvent : IDomainEvent
    {
        public IpoCanceledEvent(string plant, Guid sourceGuid, Invitation invitation)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Invitation = invitation;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Invitation Invitation { get; }
    }
}
