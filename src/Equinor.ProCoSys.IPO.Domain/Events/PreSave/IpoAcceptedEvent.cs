using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoAcceptedEvent : IDomainEvent
    {
        public IpoAcceptedEvent(string plant, Guid sourceGuid, Invitation invitation, Participant participant)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Participant = participant;
            Invitation = invitation;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Participant Participant { get; }
        public Invitation Invitation { get; set; }
    }
}
