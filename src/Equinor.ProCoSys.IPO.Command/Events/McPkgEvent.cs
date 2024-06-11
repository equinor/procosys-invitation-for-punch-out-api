using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public record McPkgEvent
    (
        Guid Guid,
        Guid ProCoSysGuid, 
        string Plant, 
        string ProjectName, 
        Guid McPkgGuid, 
        Guid InvitationGuid, 
        DateTime CreatedAtUtc)
    : IMcPkgEventV1 { }
