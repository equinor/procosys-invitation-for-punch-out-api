using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using global::Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using System.Collections;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;

namespace Equinor.ProCoSys.IPO.Command.McPkgCommands.FillMcPkgPcsGuids
{
    public class FillPCSGuidsCommandHandler : IRequestHandler<FillMcPkgPCSGuidsCommand, Result<Unit>>
    {
        private readonly ILogger<FillMcPkgPCSGuidsCommand> _logger;
        private readonly IInvitationRepository _invitationRepository;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillPCSGuidsCommandHandler(
            ILogger<FillMcPkgPCSGuidsCommand> logger,
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            ICommPkgApiService commPkgApiService,
            IMcPkgApiService mcPkgApiService,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _commPkgApiService = commPkgApiService;
            _mcPkgApiService= mcPkgApiService;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(FillMcPkgPCSGuidsCommand request, CancellationToken cancellationToken)
        {
            var allMcPkgs = _invitationRepository.GetMcPkgsOnly();
            var count = 0;
            foreach (var mcPkg in allMcPkgs)
            {
                if (mcPkg.Guid == Guid.Empty)
                {

                    var project = await _projectRepository.GetByIdAsync(mcPkg.ProjectId);
                    IList<string> commPkgNo = new List<string>() { mcPkg.McPkgNo};

                    var mcPkgWithId = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, project.Name, commPkgNo);
                    
                    if (mcPkgWithId != null && mcPkgWithId.Count == 1)
                    {
                        var mcPkgDetails = await _mcPkgApiService.GetMcPkgByIdAsync(_plantProvider.Plant, mcPkgWithId.First().Id);
                        mcPkg.Guid = mcPkgDetails.ProCoSysGuid;
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
