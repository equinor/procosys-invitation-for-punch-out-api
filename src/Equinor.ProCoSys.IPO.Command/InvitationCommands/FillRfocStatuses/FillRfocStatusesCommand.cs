using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocStatuses
{
    public class FillRfocStatusesCommand : IRequest<Result<Unit>>
    {
        public FillRfocStatusesCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
