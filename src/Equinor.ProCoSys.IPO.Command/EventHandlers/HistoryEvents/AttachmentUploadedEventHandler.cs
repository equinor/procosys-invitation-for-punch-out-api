using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class AttachmentUploadedEventHandler : INotificationHandler<AttachmentUploadedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public AttachmentUploadedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(AttachmentUploadedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.AttachmentUploaded;
            var description = $"{eventType.GetDescription()} - '{notification.FileName}'";
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
