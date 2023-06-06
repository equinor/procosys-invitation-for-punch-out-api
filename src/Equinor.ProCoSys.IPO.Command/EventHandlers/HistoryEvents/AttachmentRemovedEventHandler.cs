using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class AttachmentRemovedEventHandler : INotificationHandler<AttachmentRemovedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public AttachmentRemovedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(AttachmentRemovedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.AttachmentRemoved;
            var description = $"{eventType.GetDescription()} - '{notification.AttachmentTitle}'";
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
