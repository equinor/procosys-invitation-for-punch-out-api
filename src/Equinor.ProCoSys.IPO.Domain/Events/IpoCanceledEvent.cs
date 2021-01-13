using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public class IpoCanceledEvent : INotification
    {
        public IpoCanceledEvent(
            string plant,
            Guid objectGuid)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
