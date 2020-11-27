using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class FunctionalRoleForCommand : IRequest<Result<Unit>>
    {
        public FunctionalRoleForCommand(
            string code,
            IList<PersonForCommand> persons,
            int? id = null,
            string rowVersion = null)
        {
            Code = code;
            Persons = persons ?? new List<PersonForCommand>();
            Id = id;
            RowVersion = rowVersion;
        }
        public string Code { get; }
        public IList<PersonForCommand> Persons { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
