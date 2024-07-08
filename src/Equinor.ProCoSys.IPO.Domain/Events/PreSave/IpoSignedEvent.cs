using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoSignedEvent : IDomainEvent
    {
        public IpoSignedEvent(string plant, Guid sourceGuid, Invitation invitation, Participant participant, Person person)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            Invitation = invitation;
            Participant = participant;
            Person = person;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Invitation Invitation { get; }
        public Participant Participant { get; }
        public Person Person { get; }

    }
}
