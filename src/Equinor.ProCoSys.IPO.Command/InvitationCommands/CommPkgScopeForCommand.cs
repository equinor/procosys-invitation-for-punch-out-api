using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class CommPkgScopeForCommand : IRequest<Result<Unit>>
    {
        public CommPkgScopeForCommand(string commPkgNo, string description, string status, int? id = null)
        {
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            Id = id;
        }

        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
        public int? Id { get; }
    }
}
