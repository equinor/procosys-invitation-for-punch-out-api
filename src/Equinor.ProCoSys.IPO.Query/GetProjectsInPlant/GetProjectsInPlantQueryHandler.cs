using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.MainApi.Project;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetProjectsInPlant
{
    public class GetProjectsInPlantQueryHandler : IRequestHandler<GetProjectsInPlantQuery, Result<List<ProCoSysProjectDto>>>
    {
        private readonly IProjectApiService _projectApiService;
        private readonly IPlantProvider _plantProvider;

        public GetProjectsInPlantQueryHandler(
            IProjectApiService projectApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _projectApiService = projectApiService;
        }

        public async Task<Result<List<ProCoSysProjectDto>>> Handle(GetProjectsInPlantQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiProjects = await _projectApiService
                .GetProjectsInPlantAsync(_plantProvider.Plant) ?? new List<ProCoSysProject>();

            var projectDtos = mainApiProjects
                .Select(project => new ProCoSysProjectDto(
                    project.Id,
                    project.Name,
                    project.Description)).ToList();

            return new SuccessResult<List<ProCoSysProjectDto>>(projectDtos);
        }
    }
}
