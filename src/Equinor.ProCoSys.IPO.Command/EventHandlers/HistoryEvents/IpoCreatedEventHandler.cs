﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.HistoryEvents
{
    public class IpoCreatedEventHandler : INotificationHandler<IpoCreatedEvent>
    {
        private readonly IHistoryRepository _historyRepository;

        public IpoCreatedEventHandler(IHistoryRepository historyRepository) 
            => _historyRepository = historyRepository;

        public Task Handle(IpoCreatedEvent notification, CancellationToken cancellationToken)
        {
            var eventType = EventType.IpoCreated;
            var description = eventType.GetDescription();
            var history = new History(notification.Plant, description, notification.SourceGuid, eventType);
            _historyRepository.Add(history);
            return Task.CompletedTask;
        }
    }
}
