using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : DomainEvent
    {
        public NoteUpdatedEvent(
            string plant,
            Guid objectGuid,
            string note) : base("Note updated")
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            Note = note;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public string Note { get; }
    }
}
