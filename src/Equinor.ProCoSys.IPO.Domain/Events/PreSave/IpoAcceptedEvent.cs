using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoAcceptedEvent : DomainEvent
    {
        public IpoAcceptedEvent(
            string plant,
            Guid objectGuid) : base("IPO accepted")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
