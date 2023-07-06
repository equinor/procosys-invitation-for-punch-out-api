using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus
{
    public class UpdateRfocAcceptedCommand : IRequest<Result<Unit>>
    {
        public UpdateRfocAcceptedCommand(string projectName, string certificateNo, string certificateType, Guid proCoSysGuid)
        {
            ProjectName = projectName;
            CertificateNo = certificateNo;
            CertificateType = certificateType;
            ProCoSysGuid = proCoSysGuid;
        }

        public string ProjectName { get; }
        public string CertificateNo { get; }
        public string CertificateType { get; }
        public Guid ProCoSysGuid { get; }
    }
}
