using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.McPkgCommands.FillMcPkgCommPcsGuids
{
    public class FillPCSGuidsCommandHandler : IRequestHandler<FillMcPkgCommPkgPCSGuidsCommand, Result<Unit>>
    {
        private readonly ILogger<FillMcPkgCommPkgPCSGuidsCommand> _logger;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMcPkgApiForUserService _mcPkgApiForUserService;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillPCSGuidsCommandHandler(
            ILogger<FillMcPkgCommPkgPCSGuidsCommand> logger,
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IMcPkgApiForUserService mcPkgApiForUserService,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _mcPkgApiForUserService = mcPkgApiForUserService;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(FillMcPkgCommPkgPCSGuidsCommand request, CancellationToken cancellationToken)
        {
            var allMcPkgs = _invitationRepository.GetMcPkgsOnly();
            var count = 0;
            foreach (var mcPkg in allMcPkgs)
            {
                if (mcPkg.CommPkgGuid == Guid.Empty)
                {

                    var project = await _projectRepository.GetByIdAsync(mcPkg.ProjectId);
                    IList<string> mcPkgNo = new List<string>() { mcPkg.McPkgNo };

                    var mcPkgResult = await _mcPkgApiForUserService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, project.Name, mcPkgNo, cancellationToken);

                    if (mcPkgResult != null && mcPkgResult.Count == 1)
                    {
                        mcPkg.CommPkgGuid = mcPkgResult.First().CommPkgGuid;
                        _logger.LogInformation($"FillMcPkgCommPkgPCSGuidsCommand: McPkg {mcPkg.McPkgNo} updated with CommPkgGuid: {mcPkg.CommPkgGuid}");
                        count++;
                    }
                    else
                    {
                        _logger.LogError($"FillMcPkgCommPkgPCSGuidsCommand: McPkg {mcPkg.McPkgNo} not found.");
                    }
                }
            }

            if (request.SaveChanges && count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"FillMcPkgCommPkgPCSGuidsCommand: {count} McPks updated");
            }
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
