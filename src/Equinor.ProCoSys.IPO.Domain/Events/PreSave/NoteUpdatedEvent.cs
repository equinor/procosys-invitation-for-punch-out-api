using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : DomainEvent
    {
        public NoteUpdatedEvent(
            string plant,
            Guid sourceGuid,
            string note) : base("Note updated")
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
