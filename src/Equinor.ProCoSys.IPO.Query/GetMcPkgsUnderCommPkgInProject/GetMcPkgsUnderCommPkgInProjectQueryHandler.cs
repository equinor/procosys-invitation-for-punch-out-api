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
        private readonly IMcPkgApiForUserService _mcPkgApiForUserService;
        private readonly IPlantProvider _plantProvider;

        public GetMcPkgsUnderCommPkgInProjectQueryHandler(
            IMcPkgApiForUserService mcPkgApiForUserService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _mcPkgApiForUserService = mcPkgApiForUserService;
        }

        public async Task<Result<List<ProCoSysMcPkgDto>>> Handle(GetMcPkgsUnderCommPkgInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiMcPkgs = await _mcPkgApiForUserService
                .GetMcPkgsByCommPkgNoAndProjectNameAsync(
                   _plantProvider.Plant, request.ProjectName,
                   request.CommPkgNo,
                   cancellationToken)
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
