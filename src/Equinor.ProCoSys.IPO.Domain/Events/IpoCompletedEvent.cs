using System;
using MediatR;

namespace Equinor.Procosys.IPO.Domain.Events
{
    public class IpoCompletedEvent : INotification
    {
        public IpoCompletedEvent(
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
