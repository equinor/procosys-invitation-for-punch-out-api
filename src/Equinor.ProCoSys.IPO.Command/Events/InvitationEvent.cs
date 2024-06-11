using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public record InvitationEvent
(
    Guid Guid,
    Guid ProCoSysGuid,
    string Plant,
    string ProjectName,
    int Id,
    DateTime CreatedAtUtc,
    Guid CreatedByOid,
    DateTime? ModifiedAtUtc,
    string Title,
    string Type,
    string Description,
    string Status,
    DateTime EndTimeUtc,
    string Location,
    DateTime StartTimeUtc,
    DateTime? AcceptedAtUtc,
    Guid? AcceptedByOid,
    DateTime? CompletedAtUtc,
    Guid? CompletedByOid
) : IInvitationEventV1
{ }
