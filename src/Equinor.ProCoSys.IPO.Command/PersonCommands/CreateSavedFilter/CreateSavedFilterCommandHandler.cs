using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter
{
    public class CreateSavedFilterCommandHandler : IRequestHandler<CreateSavedFilterCommand, Result<int>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProjectApiService _projectApiService;
        private readonly IProjectRepository _projectRepository;

        public CreateSavedFilterCommandHandler(
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            IPlantProvider plantProvider,
            ICurrentUserProvider currentUserProvider,
            IProjectApiService projectApiService,
            IProjectRepository projectRepository)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _currentUserProvider = currentUserProvider;
            _projectApiService = projectApiService;
            _projectRepository = projectRepository;
        }

        public async Task<Result<int>> Handle(CreateSavedFilterCommand request, CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var person = await _personRepository.GetWithSavedFiltersByOidAsync(currentUserOid);
            Project project = null;

            if (request.ProjectName != null)
            {
                var projectName = await GetProjectFromMainAsync(request.ProjectName);
                project = await _projectRepository.GetProjectOnlyByNameAsync(projectName);
            }

            if (request.DefaultFilter)
            {
                var currentDefaultFilter = person.GetDefaultFilter(project);

                if (currentDefaultFilter != null)
                {
                    currentDefaultFilter.DefaultFilter = false;
                }
            }

            var savedFilter = new SavedFilter(_plantProvider.Plant, project, request.Title, request.Criteria)
            {
                DefaultFilter = request.DefaultFilter
            };

            person.AddSavedFilter(savedFilter);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<int>(savedFilter.Id);
        }

        public async Task<string> GetProjectFromMainAsync(string projectName)
        {
            try
            {
                var project = await _projectApiService.TryGetProjectAsync(_plantProvider.Plant, projectName);
                return project.Name;
            }
            catch (Exception e)
            {
                throw new Exception($"Error: Could not get project with name '{projectName}'", e);
            }
        }
    }
}
