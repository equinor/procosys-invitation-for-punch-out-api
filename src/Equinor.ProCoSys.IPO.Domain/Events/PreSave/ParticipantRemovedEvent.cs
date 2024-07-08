using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave;

public class ParticipantRemovedEvent : IDomainEvent
{
    public string Plant { get; }
    public Guid SourceGuid { get; }
    public Guid ParticipantGuid { get; }

    public ParticipantRemovedEvent(string plant, Guid sourceGuid, Guid participantGuid)
    {
        Plant = plant;
        SourceGuid = sourceGuid;
        ParticipantGuid = participantGuid;
    }
}
