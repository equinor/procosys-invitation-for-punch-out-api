using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoUnSignedEvent : DomainEvent
    {
        public IpoUnSignedEvent(
            string plant,
            Guid objectGuid) : base("IPO unsigned")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
