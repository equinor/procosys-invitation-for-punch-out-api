using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public record CommentEvent 
(
    Guid Guid,
    string CommentText,
    DateTime CreatedAtUtc,
    Guid CreatedByOid,
    string Plant,
    Guid InvitationGuid,
    Guid ProCoSysGuid,
    string ProjectName
) : ICommentEventV1 { }

