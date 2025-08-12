using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class NoteUpdatedEventHandler : INotificationHandler<NoteUpdatedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public NoteUpdatedEventHandler(IHistoryRepository historyRepository)
            => _historyRepository = historyRepository;

        public Task Handle(NoteUpdatedEvent notification, CancellationToken cancellationToken)
        {
            const EventType eventType = EventType.NoteUpdated;
            var description = $"{eventType.GetDescription()} - '{notification.Note}'";
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
