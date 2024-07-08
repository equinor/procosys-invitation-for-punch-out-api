namespace Equinor.ProCoSys.IPO.MessageContracts;
public interface ICommPkgEventV1 : IIntegrationEvent
{
    Guid ProCoSysGuid { get; }
    string Plant { get; }
    string ProjectName { get; }
    Guid InvitationGuid { get; }
    DateTime CreatedAtUtc { get; }
    Guid CommPkgGuid { get; }
}
