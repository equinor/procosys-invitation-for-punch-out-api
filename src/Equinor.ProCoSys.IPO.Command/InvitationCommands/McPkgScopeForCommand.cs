using MediatR;
using Polly.Fallback;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class McPkgScopeForCommand : IRequest<Result<Unit>>
    {
        public McPkgScopeForCommand(string mcPkgNo, string commPkgNo)
        {
            McPkgNo = mcPkgNo;
            CommPkgNo = commPkgNo;
        }

        public string McPkgNo { get; }
        public string CommPkgNo { get; }
    }
}
