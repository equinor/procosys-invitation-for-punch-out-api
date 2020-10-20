using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class McPkgScopeForCommand : IRequest<Result<Unit>>
    {
        public McPkgScopeForCommand(string mcPkgNo, string description, string commPkgNo)
        {
            McPkgNo = mcPkgNo;
            Description = description;
            CommPkgNo = commPkgNo;
        }

        public string McPkgNo { get; }
        public string Description { get; }
        public string CommPkgNo { get; }
    }
}
