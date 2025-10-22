using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQueryHandler : IRequestHandler<GetCommPkgsInProjectQuery, Result<ProCoSysCommPkgSearchDto>>
    {
        private readonly ICommPkgApiForUserService _commPkgApiForUserService;
        private readonly IPlantProvider _plantProvider;

        public GetCommPkgsInProjectQueryHandler(
            ICommPkgApiForUserService commPkgApiForUserService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _commPkgApiForUserService = commPkgApiForUserService;
        }

        public async Task<Result<ProCoSysCommPkgSearchDto>> Handle(GetCommPkgsInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiCommPkgSearchResult = await _commPkgApiForUserService
                .SearchCommPkgsByCommPkgNoAsync(
                    _plantProvider.Plant,
                    request.ProjectName,
                    request.StartsWithCommPkgNo,
                    cancellationToken,
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
                        commPkg.System,
                        commPkg.OperationHandoverStatus,
                        commPkg.RfocAcceptedAt)).ToList();
            }

            var commPkgSearchDto = new ProCoSysCommPkgSearchDto(mainApiCommPkgSearchResult.MaxAvailable, commPkgDtos);

            return new SuccessResult<ProCoSysCommPkgSearchDto>(commPkgSearchDto);
        }
    }
}
