using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class GetMcPkgsUnderCommPkgInProjectQueryHandler : IRequestHandler<GetMcPkgsUnderCommPkgInProjectQuery, Result<List<ProCoSysMcPkgDto>>>
    {
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetMcPkgsUnderCommPkgInProjectQueryHandler(
            IMcPkgApiService mcPkgApiService,
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
                   ?? new List<ProCoSysMcPkgOnCommPkg>();

            var mcPkgDtos = mainApiMcPkgs
                .Select(mcPkg => new ProCoSysMcPkgDto(
                    mcPkg.Id,
                    mcPkg.McPkgNo,
                    mcPkg.Description,
                    mcPkg.DisciplineCode,
                    mcPkg.System,
                    mcPkg.OperationHandoverStatus,
                    mcPkg.M01,
                    mcPkg.M02,
                    mcPkg.Status,
                    mcPkg.RfocAcceptedAt)).ToList();

            return new SuccessResult<List<ProCoSysMcPkgDto>>(mcPkgDtos);
        }
    }
}
