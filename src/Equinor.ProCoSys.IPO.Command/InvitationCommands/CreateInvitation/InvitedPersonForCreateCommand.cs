using System;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class InvitedPersonForCreateCommand : IInvitedPersonForCommand
    {
        public InvitedPersonForCreateCommand(
            Guid azureOid,
            bool required)
        {
            AzureOid = azureOid;
            Required = required;
        }
     
        public Guid AzureOid { get; }
        public bool Required { get; }
    }
}
