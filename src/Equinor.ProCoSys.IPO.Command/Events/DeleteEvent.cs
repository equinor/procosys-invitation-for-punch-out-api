using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.Events;

public class DeleteEvent : IDeleteEventV1
{
    public string EntityType { get; set; }
    public string Plant { get; set; }
    public Guid ProCoSysGuid { get; set; }
    public string Behavior => "delete"; //TODO: To be used by FamWebJob TieMapper, if we're not using FamWebJob then it can probably be removed
    public Guid Guid => ProCoSysGuid;
}
