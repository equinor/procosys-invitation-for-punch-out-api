using System;
using MediatR;

namespace Equinor.Procosys.IPO.Domain.Events
{
    public class IpoSignedEvent : INotification
    {
        public IpoSignedEvent(
            string plant,
            Guid objectGuid,
            int objectId)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            ObjectId = objectId;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public int ObjectId { get; }
    }
}
