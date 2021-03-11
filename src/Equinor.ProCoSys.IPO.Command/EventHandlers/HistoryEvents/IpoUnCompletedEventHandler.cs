using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoUnCompletedEventHandler : INotificationHandler<IpoUnCompletedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoUnCompletedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(IpoUnCompletedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoUncompleted;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
