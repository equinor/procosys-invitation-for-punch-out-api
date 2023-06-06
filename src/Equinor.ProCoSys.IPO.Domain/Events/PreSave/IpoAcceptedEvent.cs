using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoAcceptedEvent : DomainEvent
    {
        public IpoAcceptedEvent(
            string plant,
            Guid sourceGuid) : base("IPO accepted")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
