using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.ProjectCommands.FillProjectPcsGuids
{
    public class FillProjectPCSGuidsCommand : IRequest<Result<Unit>>
    {
        public FillProjectPCSGuidsCommand(bool saveChanges) => SaveChanges = saveChanges;

        public bool SaveChanges { get; }
    }
}
