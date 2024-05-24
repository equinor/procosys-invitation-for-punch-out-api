using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class ParticipantEvent : IParticipantEventV1
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
    public Guid Guid => ProCoSysGuid;
}
