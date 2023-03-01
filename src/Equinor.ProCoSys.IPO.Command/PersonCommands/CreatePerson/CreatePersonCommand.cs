using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson
{
    public class CreatePersonCommand : IRequest<Result<Unit>>
    {
        public CreatePersonCommand(Guid oid) => Oid = oid;

        public Guid Oid { get; }
    }
}
