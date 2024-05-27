namespace Equinor.ProCoSys.IPO.Command.Events;

public class CommentDeleteEvent : DeleteEvent
{
    public override string EntityType => "Comment"; //TODO: Move to constant

}
