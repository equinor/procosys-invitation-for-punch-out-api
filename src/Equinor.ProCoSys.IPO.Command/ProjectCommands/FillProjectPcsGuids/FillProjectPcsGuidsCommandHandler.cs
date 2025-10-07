using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.ProjectCommands.FillProjectPcsGuids
{
    public class FillPCSGuidsCommandHandler : IRequestHandler<FillProjectPCSGuidsCommand, Result<Unit>>
    {
        private readonly ILogger<FillProjectPCSGuidsCommand> _logger;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IProjectApiForUsersService _projectApiForUsersService;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillPCSGuidsCommandHandler(
            ILogger<FillProjectPCSGuidsCommand> logger,
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IProjectApiForUsersService projectApiForUsersService,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _projectApiForUsersService = projectApiForUsersService;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(FillProjectPCSGuidsCommand request, CancellationToken cancellationToken)
        {
            var allProjects = await _projectRepository.GetAllAsync();
            var count = 0;
            foreach (var project in allProjects)
            {
                if (project.Guid == Guid.Empty)
                {
                    var projectDetails = await _projectApiForUsersService.TryGetProjectAsync(_plantProvider.Plant, project.Name);

                    if (projectDetails != null)
                    {
                        project.Guid = projectDetails.ProCoSysGuid;
                        _logger.LogInformation($"FillProjectPCSGuids: Project updated: {project.Name}");
                        count++;
                    }
                }
            }

            if (request.SaveChanges && count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"FillProjectPCSGuids: {count} Projects updated");
            }
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
