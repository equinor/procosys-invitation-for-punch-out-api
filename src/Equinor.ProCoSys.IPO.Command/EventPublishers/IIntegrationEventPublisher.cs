using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.EventPublishers;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken) where T : class, IIntegrationEvent;
}
