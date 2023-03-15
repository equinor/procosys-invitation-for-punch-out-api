using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class AttendedStatusUpdatedEventHandler : INotificationHandler<AttendedStatusUpdatedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public AttendedStatusUpdatedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(AttendedStatusUpdatedEvent notification, CancellationToken cancellationToken)
        {
            const EventType eventType = EventType.AttendedStatusUpdated;
            var history = new History(notification.Plant, eventType.GetDescription(), notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
