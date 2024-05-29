﻿using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public record InvitationEvent : IInvitationEventV1

{
    public Guid Guid { get; set; }
    public Guid ProCoSysGuid { get; init; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public int Id { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedByOid { get; init; }
    public DateTime? ModifiedAtUtc { get; init; }
    public string Title { get; init; }
    public string Type { get; init; }
    public string Description { get; init; }
    public string Status { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string Location { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime? AcceptedAtUtc { get; init; }
    public Guid? AcceptedByOid { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public Guid? CompletedByOid { get; init; }
}
