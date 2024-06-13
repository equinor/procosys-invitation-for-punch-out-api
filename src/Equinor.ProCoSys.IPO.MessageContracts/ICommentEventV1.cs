namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface ICommentEventV1 : IIntegrationEvent
{
    public Guid ProCoSysGuid { get; init; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public string CommentText { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedByOid { get; init; }
    public Guid InvitationGuid { get; init; }
}
