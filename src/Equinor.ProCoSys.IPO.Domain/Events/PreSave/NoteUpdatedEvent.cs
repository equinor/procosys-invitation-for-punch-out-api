using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : IDomainEvent
    {
        public NoteUpdatedEvent(string plant, Guid sourceGuid, string note)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Note = note;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public string Note { get; }
    }
}
