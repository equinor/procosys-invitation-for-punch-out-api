using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class AttachmentRemovedEventHandler : INotificationHandler<AttachmentRemovedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IInvitationRepository _invitationRepository;

        public AttachmentRemovedEventHandler(IHistoryRepository historyRepository, IInvitationRepository invitationRepository)
        {
            _historyRepository = historyRepository;
            _invitationRepository = invitationRepository;
        }

        public Task Handle(AttachmentRemovedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.AttachmentRemoved;
            var ipo = _invitationRepository.GetByIdAsync(notification.InvitationId).Result;
            var description = $"{eventType.GetDescription()} - '{notification.AttachmentTitle}' removed from '{ipo.Title}'";
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
