using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class StatusResetEventHandler : INotificationHandler<StatusResetEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public StatusResetEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(StatusResetEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.StatusReset;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
