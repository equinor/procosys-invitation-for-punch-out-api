using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class PersonForCommand
    {
        public PersonForCommand(
            Guid? azureOid,
            string email,
            bool required,
            int? id = null,
            string rowVersion = null)
        {
            AzureOid = azureOid;
            Email = email;
            Required = required;
            Id = id;
            RowVersion = rowVersion;
        }
        public Guid? AzureOid { get; }
        public string Email { get; }
        public bool Required { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
