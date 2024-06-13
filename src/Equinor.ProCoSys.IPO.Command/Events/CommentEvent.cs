using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class CommentEvent : ICommentEventV1
{
    public CommentEvent(Guid guid,
        string commentText,
        DateTime createdAtUtc,
        Guid createdByGuid,
        Guid invitationGuid,
        string plant,
        string projectName)
    {
        Guid = guid;
        CommentText = commentText;
        CreatedAtUtc = createdAtUtc;
        CreatedByOid = createdByGuid;
        Plant = plant;
        InvitationGuid = invitationGuid;
        ProjectName = projectName;
    }

    public Guid Guid { get; }
    public string CommentText { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid CreatedByOid { get; }
    public string Plant { get; }
    public Guid InvitationGuid { get; }
    public Guid ProCoSysGuid => Guid;
    public string ProjectName { get; }
}
