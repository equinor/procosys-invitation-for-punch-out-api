using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class PersonForCommand : IRequest<Result<Unit>>
    {
        public PersonForCommand(
            Guid? azureOid,
            string firstName,
            string lastName,
            string email,
            bool required,
            int? id = null,
            string rowVersion = null)
        {
            AzureOid = azureOid;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Required = required;
            Id = id;
            RowVersion = rowVersion;
        }
        public Guid? AzureOid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
        public int? Id { get; set; }
        public string RowVersion { get; set; }
    }
}
