using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;
public record CommPkgEvent 
(
    Guid Guid,
    Guid ProCoSysGuid, 
    string Plant, 
    string ProjectName, 
    Guid CommPkgGuid, 
    Guid InvitationGuid, 
    DateTime CreatedAtUtc
):  ICommPkgEventV1 { }
