using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillProjects
{
    public class FillProjectsCommand : IRequest<Result<IEnumerable<string>>>
    {
        public FillProjectsCommand(bool dryRun) => DryRun = dryRun;

        public bool DryRun { get; }
    }
}
