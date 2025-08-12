using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;
public class CommPkgEvent : ICommPkgEventV1
{
    public CommPkgEvent(Guid guid,
        string plant,
        string projectName,
        Guid commPkgGuid,
        Guid invitationGuid,
        DateTime createdAtUtc)
    {
        Guid = guid;
        Plant = plant;
        ProjectName = projectName;
        CommPkgGuid = commPkgGuid;
        InvitationGuid = invitationGuid;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Guid { get; }
    public string Plant { get; }
    public string ProjectName { get; }
    public Guid CommPkgGuid { get; }
    public Guid InvitationGuid { get; }
    public DateTime CreatedAtUtc { get; }
    public Guid ProCoSysGuid => Guid;
}
