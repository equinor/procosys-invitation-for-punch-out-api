using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class InvitedPersonForCreateCommand : IInvitedPersonForCommand
    {
        public InvitedPersonForCreateCommand(
            Guid? azureOid,
            string email,
            bool required)
        {
            AzureOid = azureOid;
            Email = email;
            Required = required;
        }
     
        public Guid? AzureOid { get; }
        public string Email { get; }
        public bool Required { get; }
    }
}
