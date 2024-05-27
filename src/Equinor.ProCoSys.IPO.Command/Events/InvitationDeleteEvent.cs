namespace Equinor.ProCoSys.IPO.Command.Events;

public class InvitationDeleteEvent : DeleteEvent
{
    public override string EntityType => "Invitation"; //TODO: Move to constant

}
