using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CommPkgScopeForCommand : IRequest<Result<Unit>>
    {
        public CommPkgScopeForCommand(string commPkgNo, string description, string status)
        {
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
        }

        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
    }
}
