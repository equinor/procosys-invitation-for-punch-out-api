using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnCompletedEvent : DomainEvent
    {
        public IpoUnCompletedEvent(
            string plant,
            Guid sourceGuid) : base("IPO uncompleted")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
