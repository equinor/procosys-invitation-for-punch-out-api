using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class PersonForCommand : IRequest<Result<Unit>>
    {
        public PersonForCommand(
            Guid azureOid,
            string firstName,
            string lastName,
            string email,
            bool required)
        {
            AzureOid = azureOid;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Required = required;
        }
        public Guid AzureOid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
    }
}
