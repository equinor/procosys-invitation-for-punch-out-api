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

namespace Equinor.ProCoSys.IPO.Command.McPkgCommands.FillMcPkgPcsGuids
{
    public class FillPCSGuidsCommandHandler : IRequestHandler<FillMcPkgPCSGuidsCommand, Result<Unit>>
    {
        private readonly ILogger<FillMcPkgPCSGuidsCommand> _logger;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMcPkgApiForApplicationService _mcPkgApi;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillPCSGuidsCommandHandler(
            ILogger<FillMcPkgPCSGuidsCommand> logger,
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IMcPkgApiForApplicationService mcPkgApi,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _mcPkgApi = mcPkgApi;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(FillMcPkgPCSGuidsCommand request, CancellationToken cancellationToken)
        {
            var allMcPkgs = _invitationRepository.GetMcPkgsOnly();
            var count = 0;
            foreach (var mcPkg in allMcPkgs)
            {
                if (mcPkg.McPkgGuid == Guid.Empty)
                {

                    var project = await _projectRepository.GetByIdAsync(mcPkg.ProjectId);
                    IList<string> commPkgNo = new List<string>() { mcPkg.McPkgNo };

                    var mcPkgWithId = await _mcPkgApi.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, project.Name, commPkgNo);

                    if (mcPkgWithId != null && mcPkgWithId.Count == 1)
                    {
                        var mcPkgDetails = await _mcPkgApi.GetMcPkgByIdAsync(_plantProvider.Plant, mcPkgWithId.First().Id, cancellationToken);
                        mcPkg.McPkgGuid = mcPkgDetails.ProCoSysGuid;
                        _logger.LogInformation($"FillMcPkgPCSGuids: McPkg updated: {mcPkg.McPkgNo}");
                        count++;
                    }
                }
            }

            if (request.SaveChanges && count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"FillMcPkgPCSGuids: {count} McPks updated");
            }
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
