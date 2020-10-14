using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class FunctionalRoleForCommand : IRequest<Result<Unit>>
    {
        public FunctionalRoleForCommand(
            string code,
            string email,
            bool usePersonalEmail,
            IList<PersonForCommand> persons)
        {
            Code = code;
            Email = email;
            UsePersonalEmail = usePersonalEmail;
            Persons = persons;
        }
        public string Code { get; set; }
        public string Email { get; set; }
        public bool UsePersonalEmail { get; set; }
        public IList<PersonForCommand> Persons { get; set; }
    }
}
