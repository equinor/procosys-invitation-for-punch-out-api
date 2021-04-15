using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoCompletedEvent : INotification
    {
        public IpoCompletedEvent(
            string plant,
            Guid objectGuid,
            Invitation invitation)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            Invitation = invitation;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public Invitation Invitation { get; }
    }
}
