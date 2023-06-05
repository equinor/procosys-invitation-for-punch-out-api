using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCanceledEvent : DomainEvent
    {
        public IpoCanceledEvent(
            string plant,
            Guid objectGuid) : base("IPO canceled")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
