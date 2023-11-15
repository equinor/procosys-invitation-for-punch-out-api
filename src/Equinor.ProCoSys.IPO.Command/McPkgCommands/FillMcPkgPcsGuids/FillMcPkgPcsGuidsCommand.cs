using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.McPkgCommands.FillMcPkgPcsGuids
{
    public class FillMcPkgPCSGuidsCommand : IRequest<Result<Unit>>
    {
        public FillMcPkgPCSGuidsCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
