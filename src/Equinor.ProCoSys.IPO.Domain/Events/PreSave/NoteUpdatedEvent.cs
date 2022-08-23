using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : INotification
    {
        public NoteUpdatedEvent(
            string plant,
            Guid objectGuid,
            string note)
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
