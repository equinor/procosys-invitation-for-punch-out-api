using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoUnAcceptedEvent : INotification
    {
        public IpoUnAcceptedEvent(
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
