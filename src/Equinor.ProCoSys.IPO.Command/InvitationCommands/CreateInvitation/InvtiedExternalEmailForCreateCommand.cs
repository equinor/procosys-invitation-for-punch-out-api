namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class InvtiedExternalEmailForCreateCommand : IInvitedExternalEmailForCommand
    {
        public InvtiedExternalEmailForCreateCommand(string email) => Email = email;

        public string Email { get; }
    }
}
