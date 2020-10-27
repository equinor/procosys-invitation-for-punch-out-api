using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ExternalEmailForCommand : IRequest<Result<Unit>>
    {
        public ExternalEmailForCommand(
            string email,
            int? id = null)
        {
            Email = email;
            Id = id;
        }
        public string Email { get; set; }
        public int? Id { get; set; }
    }
}
