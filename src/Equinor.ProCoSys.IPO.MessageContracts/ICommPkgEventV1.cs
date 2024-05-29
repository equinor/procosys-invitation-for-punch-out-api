namespace Equinor.ProCoSys.IPO.MessageContracts;
public interface ICommPkgEventV1 : IIntegrationEvent
{
    Guid ProCoSysGuid { get; init; }
    string Plant { get; init; }
    string ProjectName { get; init; }
    Guid InvitationGuid { get; init; }
    DateTime CreatedAtUtc { get; init; }
}
