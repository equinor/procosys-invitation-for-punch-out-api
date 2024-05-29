namespace Equinor.ProCoSys.IPO.Command.Events;

public class McPkgDeleteEvent : DeleteEvent
{
    public override string EntityType => "McPkg"; //TODO: Move to constant
}
