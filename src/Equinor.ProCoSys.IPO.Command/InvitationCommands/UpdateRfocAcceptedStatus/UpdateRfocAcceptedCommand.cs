using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus
{
    public class UpdateRfocAcceptedCommand : IRequest<Result<Unit>>
    {
        public UpdateRfocAcceptedCommand(string projectName, Guid proCoSysGuid)
        {
            ProjectName = projectName;
            ProCoSysGuid = proCoSysGuid;
        }

        public string ProjectName { get; }
        public Guid ProCoSysGuid { get; }
    }
}
