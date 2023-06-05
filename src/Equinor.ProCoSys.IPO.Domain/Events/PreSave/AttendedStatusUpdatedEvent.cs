using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class AttendedStatusUpdatedEvent : DomainEvent
    {
        public AttendedStatusUpdatedEvent(
            string plant,
            Guid objectGuid) : base("Note updated")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
