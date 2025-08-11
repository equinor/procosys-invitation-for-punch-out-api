using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class ScopeHandedOverEventHandler : INotificationHandler<ScopeHandedOverEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public ScopeHandedOverEventHandler(IHistoryRepository historyRepository)
            => _historyRepository = historyRepository;

        public Task Handle(ScopeHandedOverEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.ScopeHandedOver;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
