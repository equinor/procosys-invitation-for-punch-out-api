﻿using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class CommentEvent : ICommentEventV1
{
    public Guid ProCoSysGuid { get; init; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public string CommentText { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedByGuid { get; init; }
    public Guid InvitationGuid { get; init; }
    public Guid Guid => ProCoSysGuid;
}