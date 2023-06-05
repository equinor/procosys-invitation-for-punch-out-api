using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCompletedEvent : DomainEvent
    {
        public IpoCompletedEvent(
            string plant,
            Guid objectGuid) : base("IPO completed")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
