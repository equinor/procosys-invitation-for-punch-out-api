using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillProjects
{
    public class FillProjectsCommandHandler : IRequestHandler<FillProjectsCommand, Result<IEnumerable<string>>>
    {
        private readonly ILogger<FillProjectsCommandHandler> _logger;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectApiService _projectApiService;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillProjectsCommandHandler(
            ILogger<FillProjectsCommandHandler> logger,
            IPlantProvider plantProvider, 
            IProjectRepository projectRepository, 
            IProjectApiService projectApiService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _projectRepository = projectRepository;
            _projectApiService = projectApiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<string>>> Handle(FillProjectsCommand request, CancellationToken cancellationToken)
        {
            var allProjects = await _projectRepository.GetAllAsync();
            var allProjectsToFill = allProjects.Where(p => p.Description == "Empty").ToList();
            var updates = new List<string>();
            foreach (var project in allProjectsToFill)
            {
                var pcsProject = await _projectApiService.TryGetProjectAsync(_plantProvider.Plant, project.Name);
                if (pcsProject != null)
                {
                    _logger.LogInformation($"Found {project.Name} in {_plantProvider.Plant}");
                    project.IsClosed = pcsProject.IsClosed;
                    project.Description = pcsProject.Description;
                    updates.Add($"{pcsProject.Name} - {project.Description} - {project.IsClosed}");

                }
                else
                {
                    _logger.LogError($"Did not find {project.Name} in {_plantProvider.Plant}");
                }
            }

            if (!request.DryRun && updates.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            return new SuccessResult<IEnumerable<string>>(updates);
        }
    }
}
