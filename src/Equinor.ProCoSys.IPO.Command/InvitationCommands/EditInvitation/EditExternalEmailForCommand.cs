namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditExternalEmailForCommand : IExternalEmailForCommand
    {
        public EditExternalEmailForCommand(
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
