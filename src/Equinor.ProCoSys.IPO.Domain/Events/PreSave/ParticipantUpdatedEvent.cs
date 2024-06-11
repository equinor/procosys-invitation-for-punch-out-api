using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class ParticipantUpdatedEvent : IDomainEvent
{
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public Invitation Invitation { get; }
    public Participant Participant { get; }

    public ParticipantUpdatedEvent(string plant, Guid sourceGuid, Invitation invitation, Participant participant)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        Invitation = invitation;
        Participant = participant;
    }
}
