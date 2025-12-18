using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoUnSignedEventHandler : INotificationHandler<IpoUnSignedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoUnSignedEventHandler(IHistoryRepository historyRepository)
            => _historyRepository = historyRepository;

        public Task Handle(IpoUnSignedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoUnsigned;
            var description = eventType.GetDescription(notification.Participant, notification.Person);
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
