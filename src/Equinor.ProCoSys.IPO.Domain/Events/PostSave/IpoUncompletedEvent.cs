using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoUnCompletedEvent : INotification
    {
        public IpoUnCompletedEvent(
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
