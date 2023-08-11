using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocVoidedStatus
{
    public class UpdateRfocVoidedCommand : IRequest<Result<Unit>>
    {
        public UpdateRfocVoidedCommand(string projectName, Guid proCoSysGuid)
        {
            ProjectName = projectName;
            ProCoSysGuid = proCoSysGuid;
        }

        public string ProjectName { get; }
        public Guid ProCoSysGuid { get; }
    }
}
