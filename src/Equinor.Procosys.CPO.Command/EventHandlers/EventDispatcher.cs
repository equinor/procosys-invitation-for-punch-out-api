using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.CPO.Domain;
using Equinor.Procosys.CPO.Domain.Events;
using MediatR;

namespace Equinor.Procosys.CPO.Command.EventHandlers
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IMediator _mediator;

        public EventDispatcher(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        public async Task DispatchAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            var entityList = entities.ToList();

            var domainEvents = entityList
                .SelectMany(x => x.DomainEvents)
                .ToList();

            entityList
                .ForEach(entity => entity.ClearDomainEvents());

            var tasks = domainEvents
                .Select(async (domainEvent) =>
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                });

            await Task.WhenAll(tasks);
        }
    }
}
