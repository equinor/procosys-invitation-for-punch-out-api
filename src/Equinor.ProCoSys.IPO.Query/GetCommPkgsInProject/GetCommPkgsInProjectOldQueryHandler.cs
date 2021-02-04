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
    public class GetCommPkgsInProjectOldQueryHandler : IRequestHandler<GetCommPkgsInProjectOldQuery, Result<IList<ProCoSysCommPkgDto>>>
    {
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IPlantProvider _plantProvider;

        public GetCommPkgsInProjectOldQueryHandler(
            ICommPkgApiService commPkgApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _commPkgApiService = commPkgApiService;
        }

        public async Task<Result<IList<ProCoSysCommPkgDto>>> Handle(GetCommPkgsInProjectOldQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiCommPkgSearchResult = await _commPkgApiService
                .SearchCommPkgsByCommPkgNoAsync(
                    _plantProvider.Plant,
                    request.ProjectName,
                    request.StartsWithCommPkgNo);

            var commPkgDtos = new List<ProCoSysCommPkgDto>();

            if (mainApiCommPkgSearchResult.Items != null)
            {
                commPkgDtos = mainApiCommPkgSearchResult.Items
                    .Select(commPkg => new ProCoSysCommPkgDto(
                        commPkg.Id,
                        commPkg.CommPkgNo,
                        commPkg.Description,
                        commPkg.CommStatus)).ToList();
            }


            return new SuccessResult<IList<ProCoSysCommPkgDto>>(commPkgDtos);
        }
    }
}
