using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class InvitationDeleteEvent : IDeleteEventV1
{
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string Behavior => "delete";

    public Guid Guid => ProCoSysGuid;
}
