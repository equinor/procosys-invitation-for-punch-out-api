namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IDeleteEventV1 : IIntegrationEvent
{
    string Plant { get; init; }
    Guid ProCoSysGuid { get; init; }
    string Behavior { get; }
}
