using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.Procosys.IPO.Domain.Events;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoCreatedEventHandler : INotificationHandler<IpoCreatedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IInvitationRepository _invitationRepository;

        public IpoCreatedEventHandler(IHistoryRepository historyRepository, IInvitationRepository invitationRepository)
        {
            _historyRepository = historyRepository;
            _invitationRepository = invitationRepository;
        }

        public Task Handle(IpoCreatedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoCreated;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.ObjectGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
