namespace Equinor.ProCoSys.IPO.MessageContracts;

public interface ICommentEventV1 : IIntegrationEvent
{
    public Guid ProCoSysGuid { get; }
    public string Plant { get; }    
    public string ProjectName { get; }  
    public string CommentText { get; }          
    public DateTime CreatedAtUtc { get; }
    public Guid CreatedByOid { get; }
    public Guid InvitationGuid { get; }
}
