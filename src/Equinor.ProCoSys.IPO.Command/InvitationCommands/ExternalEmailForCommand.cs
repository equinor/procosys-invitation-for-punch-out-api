using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ExternalEmailForCommand : IRequest<Result<Unit>>
    {
        public ExternalEmailForCommand(
            string email,
            int? id = null,
            string rowVersion = null)
        {
            Email = email;
            Id = id;
            RowVersion = rowVersion;
        }
        public string Email { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
