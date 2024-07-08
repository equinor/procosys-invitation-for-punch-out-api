using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class NoteUpdatedEvent : IDomainEvent
    {
        public NoteUpdatedEvent(string plant, Guid sourceGuid, Participant participant, Invitation invitation, string note)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Participant = participant;
            Invitation = invitation;
            Note = note;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Participant Participant { get; }
        public Invitation Invitation { get; }
        public string Note { get; }
    }
}
