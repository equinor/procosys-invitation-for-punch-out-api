namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IParticipantEventV1 : IIntegrationEvent
{
    public Guid ProCoSysGuid { get; }
    public string Plant { get; }
    public string ProjectName { get; }
    public string Organization { get; }
    public string Type { get; }
    public string FunctionalRoleCode { get; }
    public Guid? AzureOid { get; }
    public int SortKey { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid InvitationGuid { get; }
    public DateTime? ModifiedAtUtc { get; }
    public bool Attended { get; }
    public string? Note { get; }
    public DateTime? SignedAtUtc { get; }
    public Guid? SignedByOid { get; }
}
