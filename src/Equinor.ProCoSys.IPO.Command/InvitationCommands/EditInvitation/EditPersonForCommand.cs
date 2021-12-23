using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditPersonForCommand : IPersonForCommand
    {
        public EditPersonForCommand(
            int? id,
            Guid? azureOid,
            string email,
            bool required,
            string rowVersion)
        {
            Id = id;
            AzureOid = azureOid;
            Email = email;
            Required = required;
            RowVersion = rowVersion;
        }

        // null for Id will add new
        public int? Id { get; }
        public Guid? AzureOid { get; }
        public string Email { get; }
        public bool Required { get; }
        public string RowVersion { get; }
    }
}
