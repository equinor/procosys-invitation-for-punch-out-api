using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocGuids
{
    public class FillCertificateInfoCommand : IRequest<Result<Unit>>
    {
        public FillCertificateInfoCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
