using MediatR;
using Polly.Fallback;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class McPkgScopeForCommand : IRequest<Result<Unit>>
    {
        public McPkgScopeForCommand(string mcPkgNo, string description, string commPkgNo, int? id = null)
        {
            McPkgNo = mcPkgNo;
            Description = description;
            CommPkgNo = commPkgNo;
            Id = id;
        }

        public string McPkgNo { get; }
        public string Description { get; }
        public string CommPkgNo { get; }
        public int? Id { get; }
    }
}
