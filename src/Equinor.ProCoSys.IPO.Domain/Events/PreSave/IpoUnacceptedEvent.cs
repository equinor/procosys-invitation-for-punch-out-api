using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnAcceptedEvent : DomainEvent
    {
        public IpoUnAcceptedEvent(
            string plant,
            Guid objectGuid) : base("IPO unaccepted")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
