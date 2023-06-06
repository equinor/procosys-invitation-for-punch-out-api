using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnSignedEvent : DomainEvent
    {
        public IpoUnSignedEvent(
            string plant,
            Guid sourceGuid) : base("IPO unsigned")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
