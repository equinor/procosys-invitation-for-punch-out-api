using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocStatuses
{
    public class FillRfocStatusesCommandHandler : IRequestHandler<FillRfocStatusesCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FillRfocStatusesCommandHandler> _logger;

        public FillRfocStatusesCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ILogger<FillRfocStatusesCommandHandler> logger,
            IProjectRepository projectRepository,
            IMcPkgApiService mcPkgApiService,
            ICommPkgApiService commPkgApiService)
        {
            _invitationRepository = invitationRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _mcPkgApiService = mcPkgApiService;
            _commPkgApiService = commPkgApiService;
        }

        public async Task<Result<Unit>> Handle(FillRfocStatusesCommand request, CancellationToken cancellationToken)
        {
            // THIS CODE WAS WRITTEN TO RUN A ONETIME TRANSFORMATION WHEN WE INTRODUCED RfocAccepted on McPkgs and CommPkgs,
            // and new status (ScopeHandedOver) on invitation
            // WE KEEP THE CODE ... MAYBE WE WANT TO DO SIMILAR STUFF LATER

            var allProjects = await _projectRepository.GetAllAsync();
            var invitations = _invitationRepository.GetInvitationsForSynchronization();

            var mcPkgsUpdatedCount = 0;
            var commPkgsUpdatedCount = 0;
            var invitationsSetToScopeHandedOverCount = 0;

            foreach (var project in allProjects)
            {
                var invitationsInProject = invitations.Where(i => i.ProjectId == project.Id).ToList();

                mcPkgsUpdatedCount += await HandleMcPkgsAsync(invitationsInProject, project, mcPkgsUpdatedCount);
                commPkgsUpdatedCount += await HandleCommPkgsAsync(invitationsInProject, project, commPkgsUpdatedCount);
                invitationsSetToScopeHandedOverCount += HandleInvitationStatus(invitationsInProject, invitationsSetToScopeHandedOverCount);
                _logger.LogInformation($"FillRfocStatuses: Project updated: {project.Name}");
            }

            if (mcPkgsUpdatedCount > 0 || commPkgsUpdatedCount > 0)
            {
                if (request.SaveChanges)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                _logger.LogInformation($"McPkgs updated with RfocAccepted: {mcPkgsUpdatedCount}");
                _logger.LogInformation($"CommPkgs updated with RfocAccepted: {commPkgsUpdatedCount}");
                _logger.LogInformation($"Invitations set to ScopeHandedOver: {invitationsSetToScopeHandedOverCount}");
            }

            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task<int> HandleMcPkgsAsync(List<Invitation> invitations, Project project, int count)
        {
            var mcPkgsInProject = invitations.Where(i => i.Type == DisciplineType.DP)
                .SelectMany(i => i.McPkgs)
                .ToList();

            var mcPkgNosInProject = mcPkgsInProject.Select(m => m.McPkgNo).Distinct().ToList();

            var pcsMcPkgs = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(project.Plant, project.Name, mcPkgNosInProject);

            foreach (var pcsMcPkg in pcsMcPkgs)
            {
                if (pcsMcPkg.OperationHandoverStatus == "ACCEPTED")
                {
                    var mcPkgsToUpdate = mcPkgsInProject.Where(m => m.McPkgNo == pcsMcPkg.McPkgNo).ToList();
                    foreach (var mcPkg in mcPkgsToUpdate)
                    {
                        mcPkg.RfocAccepted = true;
                        count++;
                    }
                }
            }

            return count;
        }

        private async Task<int> HandleCommPkgsAsync(List<Invitation> invitations, Project project, int count)
        {
            var commPkgsInProject = invitations.Where(i => i.Type == DisciplineType.MDP)
                .SelectMany(i => i.CommPkgs)
                .ToList();

            var commPkgNosInProject = commPkgsInProject.Select(c => c.CommPkgNo).Distinct().ToList();

            var pcsCommPkgs = await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(project.Plant, project.Name, commPkgNosInProject);

            foreach (var pcsCommPkg in pcsCommPkgs)
            {
                if (pcsCommPkg.OperationHandoverStatus == "ACCEPTED")
                {
                    var commPkgsToUpdate = commPkgsInProject.Where(m => m.CommPkgNo == pcsCommPkg.CommPkgNo).ToList();
                    foreach (var commPkg in commPkgsToUpdate)
                    {
                        commPkg.RfocAccepted = true;
                        count++;
                    }
                }
            }

            return count;
        }

        private static int HandleInvitationStatus(List<Invitation> invitations, int count)
        {
            foreach (var invitation in invitations)
            {
                if (invitation.Type == DisciplineType.DP)
                {
                    if (invitation.McPkgs.All(m => m.RfocAccepted))
                    {
                        invitation.ScopeHandedOver();
                        count++;
                    }
                }
                else
                {
                    if (invitation.CommPkgs.All(c => c.RfocAccepted))
                    {
                        invitation.ScopeHandedOver();
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
