using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class CommentAddedEventHandler : INotificationHandler<CommentAddedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public CommentAddedEventHandler(IHistoryRepository historyRepository)
            => _historyRepository = historyRepository;

        public Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
        {
            const EventType eventType = EventType.CommentAdded;
            var history = new History(notification.Plant, eventType.GetDescription(), notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
