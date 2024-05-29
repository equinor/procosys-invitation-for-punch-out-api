namespace Equinor.ProCoSys.IPO.Command.Events;

public class CommPkgDeleteEvent : DeleteEvent
{
    public override string EntityType => "CommPkg"; //TODO: Move to constant
}
