namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IIntegrationEvent
{
    // The entity Guid the event was published for
    Guid Guid { get; }
}
