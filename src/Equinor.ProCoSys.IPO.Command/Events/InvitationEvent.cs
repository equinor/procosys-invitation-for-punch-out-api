using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class InvitationEvent : IInvitationEventV1
{
    public InvitationEvent(Guid guid,
        string plant,
        string projectName,
        int id,
        DateTime createdAtUtc,
        Guid createdByOid,
        DateTime? modifiedAtUtc,
        string title,
        string type,
        string description,
        string status,
        DateTime endTimeUtc,
        string location,
        DateTime startTimeUtc,
        DateTime? acceptedAtUtc,
        Guid? acceptedByOid,
        DateTime? completedAtUtc,
        Guid? completedByOid)
    {
        Guid = guid;
        Plant = plant;
        ProjectName = projectName;
        Id = id;
        CreatedAtUtc = createdAtUtc;
        CreatedByOid = createdByOid;
        ModifiedAtUtc = modifiedAtUtc;
        Title = title;
        Type = type;
        Description = description;
        Status = status;
        EndTimeUtc = endTimeUtc;
        Location = location;
        StartTimeUtc = startTimeUtc;
        AcceptedAtUtc = acceptedAtUtc;
        AcceptedByOid = acceptedByOid;
        CompletedAtUtc = completedAtUtc;
        CompletedByOid = completedByOid;
    }

    public Guid Guid { get; }
    public Guid ProCoSysGuid => Guid;
    public string Plant { get; }
    public string ProjectName { get; }
    public int Id { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid CreatedByOid { get; }
    public DateTime? ModifiedAtUtc { get; }
    public string Title { get; }
    public string Type { get; }
    public string Description { get; }
    public string Status { get; }
    public DateTime EndTimeUtc { get; }
    public string Location { get; }
    public DateTime StartTimeUtc { get; }
    public DateTime? AcceptedAtUtc { get; }
    public Guid? AcceptedByOid { get; }
    public DateTime? CompletedAtUtc { get; }
    public Guid? CompletedByOid { get; }
}
