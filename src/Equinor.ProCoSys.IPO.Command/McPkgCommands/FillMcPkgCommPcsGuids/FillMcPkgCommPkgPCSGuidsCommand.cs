using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.McPkgCommands.FillMcPkgCommPcsGuids
{
    public class FillMcPkgCommPkgPCSGuidsCommand : IRequest<Result<Unit>>
    {
        public FillMcPkgCommPkgPCSGuidsCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
