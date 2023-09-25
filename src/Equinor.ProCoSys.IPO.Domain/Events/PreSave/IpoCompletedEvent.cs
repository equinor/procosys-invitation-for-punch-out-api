using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCompletedEvent : IDomainEvent
    {
        public IpoCompletedEvent(string plant, Guid sourceGuid, Participant participant)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Participant = participant;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Participant Participant { get; }

    }
}
