using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class ParticipantEvent : IParticipantEventV1
{
    public ParticipantEvent(
        Guid guid,
        string plant,
        string projectName,
        string organization,
        string type,
        string functionalRoleCode,
        Guid? azureOid,
        int sortKey,
        DateTime createdAtUtc,
        Guid invitationGuid,
        DateTime? modifiedAtUtc,
        bool attended,
        string note,
        DateTime? signedAtUtc,
        Guid? signedByOid)
    {
        Guid = guid;
        Plant = plant;
        ProjectName = projectName;
        Organization = organization;
        Type = type;
        FunctionalRoleCode = functionalRoleCode;
        AzureOid = azureOid;
        SortKey = sortKey;
        CreatedAtUtc = createdAtUtc;
        InvitationGuid = invitationGuid;
        ModifiedAtUtc = modifiedAtUtc;
        Attended = attended;
        Note = note;
        SignedAtUtc = signedAtUtc;
        SignedByOid = signedByOid;
    }

    public Guid Guid { get; }
    public Guid ProCoSysGuid => Guid;
    public string Plant { get; }
    public string ProjectName { get; }
    public string Organization { get; }
    public string Type { get; }
    public string FunctionalRoleCode { get; }
    public Guid? AzureOid { get; }
    public int SortKey { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid InvitationGuid { get; }
    public DateTime? ModifiedAtUtc { get; }
    public bool Attended { get; }
    public string Note { get; }
    public DateTime? SignedAtUtc { get; }
    public Guid? SignedByOid { get; }
}
