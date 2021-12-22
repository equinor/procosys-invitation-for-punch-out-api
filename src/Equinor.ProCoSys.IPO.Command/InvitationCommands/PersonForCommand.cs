using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class PersonForCommand
    {
        public PersonForCommand(
            Guid? azureOid,
            string email,
            bool required,
            int? id = null)
        {
            AzureOid = azureOid;
            Email = email;
            Required = required;
            Id = id;
        }
        public Guid? AzureOid { get; }
        public string Email { get; }
        public bool Required { get; }
        public int? Id { get; }
    }
}
