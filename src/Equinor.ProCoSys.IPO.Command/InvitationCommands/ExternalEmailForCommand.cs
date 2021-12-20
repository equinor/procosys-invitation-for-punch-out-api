namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ExternalEmailForCommand
    {
        public ExternalEmailForCommand(
            string email,
            int? id = null)
        {
            Email = email;
            Id = id;
        }
        public string Email { get; }
        public int? Id { get; }
    }
}
