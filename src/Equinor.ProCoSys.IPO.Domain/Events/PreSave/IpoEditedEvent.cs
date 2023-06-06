using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoEditedEvent : DomainEvent
    {
        public IpoEditedEvent(
            string plant,
            Guid sourceGuid) : base("IPO edited")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
