using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetFunctionalRoles
{
    public class GetFunctionalRolesForIpoQueryHandler : IRequestHandler<GetFunctionalRolesForIpoQuery, Result<List<ProCoSysFunctionalRoleDto>>>
    {
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IPlantProvider _plantProvider;

        public GetFunctionalRolesForIpoQueryHandler(
            IFunctionalRoleApiService functionalRoleApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _functionalRoleApiService = functionalRoleApiService;
        }

        public async Task<Result<List<ProCoSysFunctionalRoleDto>>> Handle(GetFunctionalRolesForIpoQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiFunctionalRoles = await _functionalRoleApiService
                                .GetFunctionalRolesByClassificationAsync(
                                        _plantProvider.Plant,
                                        request.Classification)
                                ?? new List<ProCoSysFunctionalRole>();

            var functionalRoleDtos = mainApiFunctionalRoles
                .Select(functionalRole => new ProCoSysFunctionalRoleDto(
                    functionalRole.Code,
                    functionalRole.Description,
                    functionalRole.Email,
                    functionalRole.InformationEmail,
                    functionalRole.UsePersonalEmail,
                    functionalRole.Persons)).ToList();

            return new SuccessResult<List<ProCoSysFunctionalRoleDto>>(functionalRoleDtos);
        }
    }
}
