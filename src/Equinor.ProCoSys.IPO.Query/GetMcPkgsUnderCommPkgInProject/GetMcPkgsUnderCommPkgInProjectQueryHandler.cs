using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class GetMcPkgsUnderCommPkgInProjectQueryHandler : IRequestHandler<GetMcPkgsUnderCommPkgInProjectQuery, Result<List<ProCoSysMcPkgDto>>>
    {
        private readonly IMainMcPkgApiService _mcPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetMcPkgsUnderCommPkgInProjectQueryHandler(
            IMainMcPkgApiService mcPkgApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _mcPkgApiService = mcPkgApiService;
        }

        public async Task<Result<List<ProCoSysMcPkgDto>>> Handle(GetMcPkgsUnderCommPkgInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiMcPkgs = await _mcPkgApiService
                .GetMcPkgsByCommPkgNoAndProjectNameAsync(
                   _plantProvider.Plant, request.ProjectName,
                   request.CommPkgNo)
                   ?? new List<ProCoSysMcPkg>();

            var mcPkgDtos = mainApiMcPkgs
                .Select(mcPkg => new ProCoSysMcPkgDto(
                    mcPkg.Id,
                    mcPkg.McPkgNo,
                    mcPkg.Description,
                    mcPkg.DisciplineCode,
                    mcPkg.CommPkgNo,
                    mcPkg.System)).ToList();

            return new SuccessResult<List<ProCoSysMcPkgDto>>(mcPkgDtos);
        }
    }
}
