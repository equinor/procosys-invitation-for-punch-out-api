using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class McPkgEvent : IMcPkgEventV1
{
    public McPkgEvent(Guid guid,
        string plant,
        string projectName,
        Guid mcPkgGuid,
        Guid invitationGuid,
        DateTime createdAtUtc)
    {
        Guid = guid;
        Plant = plant;
        ProjectName = projectName;
        McPkgGuid = mcPkgGuid;
        InvitationGuid = invitationGuid;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Guid { get; }
    public Guid ProCoSysGuid => Guid;
    public string Plant { get; }
    public string ProjectName { get; }
    public Guid McPkgGuid { get; }
    public Guid InvitationGuid { get; }
    public DateTime CreatedAtUtc { get; }
}
