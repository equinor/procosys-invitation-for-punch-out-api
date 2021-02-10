using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
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
