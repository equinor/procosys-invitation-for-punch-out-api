namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class InvitedExternalEmailForCreateCommand : IInvitedExternalEmailForCommand
    {
        public InvitedExternalEmailForCreateCommand(string email) => Email = email;

        public string Email { get; }
    }
}
