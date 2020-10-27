using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class FunctionalRoleForCommand : IRequest<Result<Unit>>
    {
        public FunctionalRoleForCommand(
            string code,
            string email,
            bool usePersonalEmail,
            IList<PersonForCommand> persons,
            int? id = null)
        {
            Code = code;
            Email = email;
            UsePersonalEmail = usePersonalEmail;
            Persons = persons ?? new List<PersonForCommand>();
            Id = id;

        }
        public string Code { get; set; }
        public string Email { get; set; }
        public bool UsePersonalEmail { get; set; }
        public IList<PersonForCommand> Persons { get; set; }
        public int? Id { get; set; }
    }
}
