using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetProjectsInPlant
{
    public class GetProjectsInPlantQuery : IRequest<Result<List<ProCoSysProjectDto>>>
    {
    }
}
