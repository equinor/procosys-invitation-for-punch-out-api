namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface IInvitationEventV1 : IIntegrationEvent
{
    public string Plant { get; }
    public string ProjectName { get; }
    public int Id { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid CreatedByOid { get; }
    public DateTime? ModifiedAtUtc { get; }
    public string Title { get; }
    public string Type { get; }
    public string Description { get; }
    public string Status { get; }
    public DateTime EndTimeUtc { get; }
    public string Location { get; }
    public DateTime StartTimeUtc { get; }
    public DateTime? AcceptedAtUtc { get; }
    public Guid? AcceptedByOid { get; }
    public DateTime? CompletedAtUtc { get; }
    public Guid? CompletedByOid { get; }

}
