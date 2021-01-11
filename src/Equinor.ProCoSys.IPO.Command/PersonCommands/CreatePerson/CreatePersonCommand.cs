using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson
{
    public class CreatePersonCommand : IRequest<Result<Unit>>
    {
        public CreatePersonCommand(Guid oid, string firstName, string lastName, string userName, string email)
        {
            Oid = oid;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            Email = email;
        }

        public Guid Oid { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string UserName { get; }
        public string Email { get; }
    }
}
