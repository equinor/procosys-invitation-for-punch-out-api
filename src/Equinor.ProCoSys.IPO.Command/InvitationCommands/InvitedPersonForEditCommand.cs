using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class InvitedPersonForEditCommand : IInvitedPersonForCommand
    {
        public InvitedPersonForEditCommand(int? id,
            Guid azureOid,
            bool required,
            string rowVersion)
        {
            Id = id;
            AzureOid = azureOid;
            Required = required;
            RowVersion = rowVersion;
        }

        // null for Id will add new
        public int? Id { get; }
        public Guid AzureOid { get; }
        public bool Required { get; }
        public string RowVersion { get; }
    }
}
