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
    public class GetCommPkgsInProjectQueryHandler : IRequestHandler<GetCommPkgsInProjectQuery, Result<ProCoSysCommPkgSearchDto>>
    {
        private readonly IMainCommPkgApiService _commPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetCommPkgsInProjectQueryHandler(
            IMainCommPkgApiService commPkgApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _commPkgApiService = commPkgApiService;
        }

        public async Task<Result<ProCoSysCommPkgSearchDto>> Handle(GetCommPkgsInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiCommPkgSearchResult = await _commPkgApiService
                .SearchCommPkgsByCommPkgNoAsync(
                    _plantProvider.Plant,
                    request.ProjectName,
                    request.StartsWithCommPkgNo,
                    request.ItemsPerPage,
                    request.CurrentPage);

            var commPkgDtos = new List<ProCoSysCommPkgDto>();

            if (mainApiCommPkgSearchResult.Items != null)
            {
                commPkgDtos = mainApiCommPkgSearchResult.Items
                    .Select(commPkg => new ProCoSysCommPkgDto(
                        commPkg.Id,
                        commPkg.CommPkgNo,
                        commPkg.Description,
                        commPkg.CommStatus,
                        commPkg.System)).ToList();
            }

            var commPkgSearchDto = new ProCoSysCommPkgSearchDto(mainApiCommPkgSearchResult.MaxAvailable, commPkgDtos);

            return new SuccessResult<ProCoSysCommPkgSearchDto>(commPkgSearchDto);
        }
    }
}
