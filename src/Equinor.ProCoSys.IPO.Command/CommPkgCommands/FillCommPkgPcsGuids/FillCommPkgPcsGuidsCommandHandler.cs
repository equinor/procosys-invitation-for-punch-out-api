using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.CommPkgCommands.FillCommPkgPcsGuids
{
    public class FillPCSGuidsCommandHandler : IRequestHandler<FillCommPkgPCSGuidsCommand, Result<Unit>>
    {
        private readonly ILogger<FillCommPkgPCSGuidsCommand> _logger;
        private readonly IInvitationRepository _invitationRepository;
        private readonly ICommPkgApiForUserService _commPkgApiForUserService;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public FillPCSGuidsCommandHandler(
            ILogger<FillCommPkgPCSGuidsCommand> logger,
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            ICommPkgApiForUserService commPkgApiForUserService,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _commPkgApiForUserService = commPkgApiForUserService;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(FillCommPkgPCSGuidsCommand request, CancellationToken cancellationToken)
        {
            var allCommPkgs = _invitationRepository.GetCommPkgsOnly();
            var count = 0;
            foreach (var commPkg in allCommPkgs)
            {
                if (commPkg.CommPkgGuid == Guid.Empty)
                {
                    var project = await _projectRepository.GetByIdAsync(commPkg.ProjectId);
                    IList<string> commPkgNo = new List<string>() { commPkg.CommPkgNo };
                    var commPkgDetails = await _commPkgApiForUserService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, project.Name, commPkgNo, cancellationToken);

                    if (commPkgDetails != null && commPkgDetails.Count == 1)
                    {
                        commPkg.CommPkgGuid = commPkgDetails.First().ProCoSysGuid;
                        _logger.LogInformation($"FillCommPkgPCSGuids: CommPkg updated: {commPkg.CommPkgNo}");
                        count++;
                    }
                }
            }

            if (request.SaveChanges && count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"FillCommPkgPCSGuids: {count} CommPks updated");
            }
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
