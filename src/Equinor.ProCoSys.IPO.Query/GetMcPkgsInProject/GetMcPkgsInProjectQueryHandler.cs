using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.MainApi.McPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsInProject
{
    public class GetMcPkgsInProjectQueryHandler : IRequestHandler<GetMcPkgsInProjectQuery, Result<List<ProCoSysMcPkgDto>>>
    {
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetMcPkgsInProjectQueryHandler(
            IMcPkgApiService mcPkgApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _mcPkgApiService = mcPkgApiService;
        }

        public async Task<Result<List<ProCoSysMcPkgDto>>> Handle(GetMcPkgsInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiMcPkgs = await _mcPkgApiService
                .SearchMcPkgsByMcPkgNoAsync(
                   _plantProvider.Plant, request.ProjectId,
                   request.StartsWithMcPkgNo)
                   ?? new List<ProCoSysMcPkg>();

            var mcPkgDtos = mainApiMcPkgs
                .Select(mcPkg => new ProCoSysMcPkgDto(
                    mcPkg.Id,
                    mcPkg.McPkgNo,
                    mcPkg.Description)).ToList();

            return new SuccessResult<List<ProCoSysMcPkgDto>>(mcPkgDtos);
        }
    }
}
