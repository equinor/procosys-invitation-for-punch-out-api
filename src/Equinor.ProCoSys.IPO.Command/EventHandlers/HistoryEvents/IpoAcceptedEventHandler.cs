using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoAcceptedEventHandler : INotificationHandler<IpoAcceptedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoAcceptedEventHandler(IHistoryRepository historyRepository)
            => _historyRepository = historyRepository;

        public Task Handle(IpoAcceptedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoAccepted;
            var description = eventType.GetDescription(notification.Participant);
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
