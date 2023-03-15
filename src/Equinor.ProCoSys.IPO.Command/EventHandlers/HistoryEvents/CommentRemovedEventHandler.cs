using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class CommentRemovedEventHandler : INotificationHandler<CommentRemovedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public CommentRemovedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(CommentRemovedEvent notification, CancellationToken cancellationToken)
        {
            const EventType eventType = EventType.CommentRemoved;
            var history = new History(notification.Plant, eventType.GetDescription(), notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
