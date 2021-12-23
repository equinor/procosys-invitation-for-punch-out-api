namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateExternalEmailForCommand : IExternalEmailForCommand
    {
        public CreateExternalEmailForCommand(string email) => Email = email;

        public string Email { get; }
    }
}
