using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocGuids
{
    public class FillRfocGuidsCommand : IRequest<Result<Unit>>
    {
        public FillRfocGuidsCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
