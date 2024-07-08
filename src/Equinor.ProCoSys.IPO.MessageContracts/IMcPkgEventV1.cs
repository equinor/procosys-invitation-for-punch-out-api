namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IMcPkgEventV1 : IIntegrationEvent
{
    Guid ProCoSysGuid { get; }
    string Plant { get; }
    string ProjectName { get; }
    Guid InvitationGuid { get; }
    DateTime CreatedAtUtc { get; }
    public Guid McPkgGuid { get; }

}
