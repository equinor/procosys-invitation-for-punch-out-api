using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class BusEventMessage
    {
        public string Plant { get; set; }
        public string Event { get; set; }
        public Guid InvitationGuid { get; set; }
        public IpoStatus IpoStatus { get; set; }
    }
}
