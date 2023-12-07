using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.CommPkgCommands.FillCommPkgPcsGuids
{
    public class FillCommPkgPCSGuidsCommand : IRequest<Result<Unit>>
    {
        public FillCommPkgPCSGuidsCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
