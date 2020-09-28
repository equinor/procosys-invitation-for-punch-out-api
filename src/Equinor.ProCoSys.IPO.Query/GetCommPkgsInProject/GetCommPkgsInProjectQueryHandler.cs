using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQueryHandler : IRequestHandler<GetCommPkgsInProjectQuery, Result<List<ProCoSysCommPkgDto>>>
    {
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetCommPkgsInProjectQueryHandler(
            ICommPkgApiService commPkgApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _commPkgApiService = commPkgApiService;
        }

        public async Task<Result<List<ProCoSysCommPkgDto>>> Handle(GetCommPkgsInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiCommPkgs = await _commPkgApiService
                .SearchCommPkgsByCommPkgNoAsync(
                   _plantProvider.Plant, request.ProjectId,
                   request.StartsWithCommPkgNo)
                   ?? new List<ProCoSysCommPkg>();

            var commPkgDtos = mainApiCommPkgs
                .Select(commPkg => new ProCoSysCommPkgDto(
                    commPkg.Id,
                    commPkg.CommPkgNo,
                    commPkg.Description,
                    commPkg.CommStatus)).ToList();

            return new SuccessResult<List<ProCoSysCommPkgDto>>(commPkgDtos);
        }
    }
}
