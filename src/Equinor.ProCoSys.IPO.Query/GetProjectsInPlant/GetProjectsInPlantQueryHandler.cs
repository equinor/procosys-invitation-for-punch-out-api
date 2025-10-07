using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetProjectsInPlant
{
    public class GetProjectsInPlantQueryHandler : IRequestHandler<GetProjectsInPlantQuery, Result<List<ProCoSysProjectDto>>>
    {
        private readonly IProjectApiForUsersService _projectApiForUsersService;
        private readonly IPlantProvider _plantProvider;

        public GetProjectsInPlantQueryHandler(
            IProjectApiForUsersService projectApiForUsersService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _projectApiForUsersService = projectApiForUsersService;
        }

        public async Task<Result<List<ProCoSysProjectDto>>> Handle(GetProjectsInPlantQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiProjects = await _projectApiForUsersService
                .GetProjectsInPlantAsync(_plantProvider.Plant, cancellationToken) ?? new List<ProCoSysProject>();

            var projectDtos = mainApiProjects
                .Select(project => new ProCoSysProjectDto(
                    project.Id,
                    project.Name,
                    project.Description)).ToList();

            return new SuccessResult<List<ProCoSysProjectDto>>(projectDtos);
        }
    }
}
