using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.LoggingEvents
{
    public class IpoNotSetToHandedOverEventHandler : INotificationHandler<IpoNotSetToHandedOverEvent>
    {
        private readonly ILogger<IpoNotSetToHandedOverEventHandler> _logger;

        public IpoNotSetToHandedOverEventHandler(ILogger<IpoNotSetToHandedOverEventHandler> logger) 
            => _logger = logger;

        public Task Handle(IpoNotSetToHandedOverEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoNotHandedOver;
            _logger.LogInformation($"{eventType.GetDescription()}. Plant: [{notification.Plant}], Guid [{notification.SourceGuid}], Current status: [{notification.Status}].");
            return Task.CompletedTask;
        }
    }
}
