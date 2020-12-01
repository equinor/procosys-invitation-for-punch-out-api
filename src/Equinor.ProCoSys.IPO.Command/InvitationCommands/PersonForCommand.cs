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
        public Guid? AzureOid { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public bool Required { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
