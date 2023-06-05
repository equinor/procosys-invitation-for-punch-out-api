using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCreatedEvent : DomainEvent
    {
        public IpoCreatedEvent(
            string plant,
            Guid objectGuid) : base("IPO created")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
