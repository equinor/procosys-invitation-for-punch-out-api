using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoNotSetToHandedOverEvent : IDomainEvent
    {
        public IpoNotSetToHandedOverEvent(string plant, Guid sourceGuid, IpoStatus status)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Status = status;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public IpoStatus Status { get; }
    }
}
