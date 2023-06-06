using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttendedStatusUpdatedEvent : DomainEvent
    {
        public AttendedStatusUpdatedEvent(
            string plant,
            Guid sourceGuid) : base("Note updated")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
