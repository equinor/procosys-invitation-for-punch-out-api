using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoCompletedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoCompleted;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
