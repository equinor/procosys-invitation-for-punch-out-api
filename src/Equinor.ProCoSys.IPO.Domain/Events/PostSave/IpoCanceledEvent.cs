using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoCanceledEvent : INotification
    {
        public IpoCanceledEvent(
            string plant,
            Guid objectGuid,
            IpoStatus status)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            Status = status;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public IpoStatus Status { get; }
    }
}
