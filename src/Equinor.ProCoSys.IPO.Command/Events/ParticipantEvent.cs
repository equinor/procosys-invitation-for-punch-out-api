using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public record ParticipantEvent
(
    Guid Guid,
    Guid ProCoSysGuid,
    string Plant,
    string ProjectName,
    string Organization,
    string Type,
    string FunctionalRoleCode,
    Guid? AzureOid,
    int SortKey,
    DateTime CreatedAtUtc,
    Guid InvitationGuid,
    DateTime? ModifiedAtUtc,
    bool Attended,
    string Note,
    DateTime? SignedAtUtc,
    Guid? SignedByOid)
    : IParticipantEventV1
{}
