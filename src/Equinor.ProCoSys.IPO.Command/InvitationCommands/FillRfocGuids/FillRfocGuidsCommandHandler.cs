using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocGuids
{
    public class FillRfocGuidsCommandHandler : IRequestHandler<FillRfocGuidsCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FillRfocGuidsCommandHandler> _logger;

        public FillRfocGuidsCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ILogger<FillRfocGuidsCommandHandler> logger,
            IProjectRepository projectRepository,
            IMcPkgApiService mcPkgApiService,
            ICommPkgApiService commPkgApiService,
            ICertificateRepository certificateRepository)
        {
            _invitationRepository = invitationRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _mcPkgApiService = mcPkgApiService;
            _commPkgApiService = commPkgApiService;
            _certificateRepository = certificateRepository;
        }

        public async Task<Result<Unit>> Handle(FillRfocGuidsCommand request, CancellationToken cancellationToken)
        {
            // THIS CODE WAS WRITTEN TO RUN A ONETIME TRANSFORMATION WHEN WE INTRODUCED Certificate table
            // WE KEEP THE CODE ... MAYBE WE WANT TO DO SIMILAR STUFF LATER

            //var projects = await _projectRepository.GetAllAsync();
            //var invitations = _invitationRepository.GetInvitationsForSynchronization();

            //var mcPkgsUpdatedCount = 0;
            //var commPkgsUpdatedCount = 0;

            //foreach (var project in projects)
            //{
            //    var invitationsInProject = invitations.Where(i => i.ProjectId == project.Id).ToList();

            //    mcPkgsUpdatedCount += await HandleMcPkgsAsync(invitationsInProject, project, cancellationToken);
            //    commPkgsUpdatedCount += await HandleCommPkgsAsync(invitationsInProject, project, cancellationToken);
            //    _logger.LogInformation($"FillRfocGuids: Project updated: {project.Name}");
            //}

            //if (mcPkgsUpdatedCount > 0 || commPkgsUpdatedCount > 0)
            //{
            //    if (request.SaveChanges)
            //    {
            //        await _unitOfWork.SaveChangesAsync(cancellationToken);
            //    }
            //    _logger.LogInformation($"McPkgs updated with RfocGuid: {mcPkgsUpdatedCount}");
            //    _logger.LogInformation($"CommPkgs updated with RfocGuid: {commPkgsUpdatedCount}");
            //}

            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task<int> HandleMcPkgsAsync(List<Invitation> invitations, Project project, CancellationToken token)
        {
            var count = 0;

            var mcPkgsInProject = invitations.Where(i => i.Type == DisciplineType.DP)
                .SelectMany(i => i.McPkgs)
                .ToList();

            if (mcPkgsInProject.Any())
            {
                var mcPkgNosInProject = mcPkgsInProject.Select(m => m.McPkgNo).Distinct().ToList();
                var pcsMcPkgs = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(project.Plant, project.Name, mcPkgNosInProject);

                foreach (var pcsMcPkg in pcsMcPkgs)
                {
                    if (pcsMcPkg.OperationHandoverStatus == "ACCEPTED" && pcsMcPkg.RfocGuid != null && pcsMcPkg.RfocGuid != Guid.Empty)
                    {
                        var mcPkgsToUpdate = mcPkgsInProject.Where(m => m.McPkgNo == pcsMcPkg.McPkgNo).ToList();
                        foreach (var mcPkg in mcPkgsToUpdate)
                        {
                            var certificate = await GetOrCreateCertificateAsync((Guid)pcsMcPkg.RfocGuid, project, token);
                            certificate.AddMcPkgRelation(mcPkg);
                            count++; 
                        }
                    }
                }
            }

            return count;
        }

        private async Task<int> HandleCommPkgsAsync(List<Invitation> invitations, Project project, CancellationToken token)
        {
            var count = 0;

            var commPkgsInProject = invitations.Where(i => i.Type == DisciplineType.MDP)
                .SelectMany(i => i.CommPkgs)
                .ToList();

            if (commPkgsInProject.Any())
            {
                var commPkgNosInProject = commPkgsInProject.Select(c => c.CommPkgNo).Distinct().ToList();
                var pcsCommPkgRfocRelations = await _commPkgApiService.GetRfocGuidsByCommPkgNosAsync(project.Plant, project.Name, commPkgNosInProject);

                foreach (var relation in pcsCommPkgRfocRelations)
                {
                    if (relation.RfocGuid != null && relation.RfocGuid != Guid.Empty)
                    {
                        var commPkgsToUpdate = commPkgsInProject.Where(m => m.CommPkgNo == relation.CommPkgNo).ToList();
                        foreach (var commPkg in commPkgsToUpdate)
                        {
                            var certificate = await GetOrCreateCertificateAsync((Guid)relation.RfocGuid, project, token);
                            certificate.AddCommPkgRelation(commPkg);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        private async Task<Certificate> GetOrCreateCertificateAsync(Guid certificateGuid, Project project, CancellationToken cancellationToken)
            => await _certificateRepository.GetCertificateByGuid(certificateGuid) ?? await AddCertificateAsync(certificateGuid, project, cancellationToken);

        private async Task<Certificate> AddCertificateAsync(Guid certificateGuid, Project project, CancellationToken cancellationToken)
        {
            var certificate = new Certificate(project.Plant, project, certificateGuid, true);
            _certificateRepository.Add(certificate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return certificate;
        }
    }
}
