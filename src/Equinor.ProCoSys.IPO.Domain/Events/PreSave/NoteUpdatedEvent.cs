using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : IDomainEvent
    {
        public NoteUpdatedEvent(string plant, Guid sourceGuid, Guid participantGuid, string note)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            ParticipantGuid = participantGuid;
            Note = note;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Guid ParticipantGuid { get; }
        public string Note { get; }
    }
}
