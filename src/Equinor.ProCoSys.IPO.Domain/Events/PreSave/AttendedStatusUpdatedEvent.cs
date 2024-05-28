using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttendedStatusUpdatedEvent : IDomainEvent
    {
        public AttendedStatusUpdatedEvent(string plant, Guid sourceGuid, Guid participantGuid)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            ParticipantGuid = participantGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Guid ParticipantGuid { get; }
    }
}
