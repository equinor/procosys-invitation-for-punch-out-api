using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttendedStatusUpdatedEvent : IDomainEvent
    {
        public AttendedStatusUpdatedEvent(string plant, Guid sourceGuid, Participant participant, Invitation invitation)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Participant = participant;
            Invitation = invitation;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Participant Participant { get; }
        public Invitation Invitation { get; }
    }
}
