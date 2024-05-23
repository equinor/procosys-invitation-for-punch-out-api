namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IDeleteEventV1 : IIntegrationEvent
{
    string EntityType { get; set; }
    string Plant { get; set; }
    Guid ProCoSysGuid { get; set; }
    string Behavior { get; }
}