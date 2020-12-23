using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public class IpoCreatedEvent : INotification
    {
        public IpoCreatedEvent(
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
