using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoCanceledEvent : DomainEvent
    {
        public IpoCanceledEvent(
            string plant,
            Guid sourceGuid) : base("IPO canceled")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
