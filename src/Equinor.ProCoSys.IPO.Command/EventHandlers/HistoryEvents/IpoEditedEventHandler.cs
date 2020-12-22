using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.Procosys.IPO.Domain.Events;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoEditedEventHandler : INotificationHandler<IpoEditedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoEditedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(IpoEditedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoEdited;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
