using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnCompletedEvent : DomainEvent
    {
        public IpoUnCompletedEvent(
            string plant,
            Guid objectGuid) : base("IPO uncompleted")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
