namespace Equinor.ProCoSys.IPO.Command.Events;

public class ParticipantDeleteEvent : DeleteEvent
{
    public override string EntityType => "Participant"; //TODO: Move to constant

}
