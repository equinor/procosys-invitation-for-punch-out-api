namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IParticipantEventV1 : IIntegrationEvent
{
    public Guid ProCoSysGuid { get; init; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public string Organization { get; init; }
    public string Type { get; init; }
    public string FunctionalRoleCode { get; init; }
    public Guid? AzureOid { get; init; }
    public int SortKey { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid InvitationGuid { get; init; }
    public DateTime? ModifiedAtUtc { get; init; }
    public bool Attended { get; init; }
    public string? Note { get; init; }
    public DateTime? SignedAtUtc { get; init; }
    public Guid? SignedByOid { get; init; }
}
