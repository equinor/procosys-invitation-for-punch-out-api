using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.Procosys.Preservation.Domain.Events;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IInvitationRepository _invitationRepository;

        public IpoCompletedEventHandler(IHistoryRepository historyRepository, IInvitationRepository invitationRepository)
        {
            _historyRepository = historyRepository;
            _invitationRepository = invitationRepository;
        }

        public Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoCompleted;
            var ipo = _invitationRepository.GetByIdAsync(notification.ObjectId).Result;
            var description = $"{eventType.GetDescription()} - '{ipo.Title}'";
            var history = new History(notification.Plant, description, notification.ObjectId, ObjectType.Ipo, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
