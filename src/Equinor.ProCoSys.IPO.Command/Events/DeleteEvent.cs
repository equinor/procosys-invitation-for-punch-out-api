using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public abstract class DeleteEvent : IDeleteEventV1
{
    public abstract string EntityType { get; } //TODO: Figure out if we should move this to separate interface
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string Behavior => "delete"; //TODO: To be used by FamWebJob TieMapper, if we're not using FamWebJob then it can probably be removed
    public Guid Guid => ProCoSysGuid;
}
