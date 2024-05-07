using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.EventPublishers
{
    public class BusEventMessage : IIntegrationEvent
    {
        public string Plant { get; set; }
        public string Event { get; set; }
        public Guid InvitationGuid { get; set; }
        public IpoStatus IpoStatus { get; set; }

        //For compatibility with the new IIntegrationEvent interface
        public Guid Guid { get => InvitationGuid; }
    }
}
