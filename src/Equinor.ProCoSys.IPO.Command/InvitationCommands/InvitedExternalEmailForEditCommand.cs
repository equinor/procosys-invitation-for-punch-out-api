namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class InvitedExternalEmailForEditCommand : IInvitedExternalEmailForCommand
    {
        public InvitedExternalEmailForEditCommand(
            int? id,
            string email,
            string rowVersion)
        {
            Id = id;
            Email = email;
            RowVersion = rowVersion;
        }

        // null for Id will add new
        public int? Id { get; }
        public string Email { get; }
        public string RowVersion { get; }
    }
}
