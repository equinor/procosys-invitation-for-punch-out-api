using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus
{
    public class UpdateRfocAcceptedCommandHandler : IRequestHandler<UpdateRfocAcceptedCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly ICertificateApiService _certificateApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly ILogger<UpdateRfocAcceptedCommandHandler> _logger;

        public UpdateRfocAcceptedCommandHandler(
            IInvitationRepository invitationRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IPlantProvider plantProvider,
            ICertificateApiService certificateApiService,
            ILogger<UpdateRfocAcceptedCommandHandler> logger,
            ICertificateRepository certificateRepository,
            IMcPkgApiService mcPkgApiService,
            ICommPkgApiService commPkgApiService)
        {
            _invitationRepository = invitationRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _certificateApiService = certificateApiService;
            _logger = logger;
            _certificateRepository = certificateRepository;
            _mcPkgApiService = mcPkgApiService;
            _commPkgApiService = commPkgApiService;
        }

        public async Task<Result<Unit>> Handle(UpdateRfocAcceptedCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetProjectOnlyByNameAsync(request.ProjectName);
            if (project == null)
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. Project {request.ProjectName} does not exists in IPO module");
                return new SuccessResult<Unit>(Unit.Value);
            }
            
            if (project.IsClosed)
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. Project {request.ProjectName} is closed in IPO module");
                return new SuccessResult<Unit>(Unit.Value);
            }

            var certificateMcPkgsModel = await _certificateApiService.GetCertificateMcPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);

            if (certificateMcPkgsModel == null)
            {
                var error = $"Certificate {request.ProCoSysGuid} McPkg scope not found";
                _logger.LogError(error);
                return new NotFoundResult<Unit>(error);
            }

            var certificateCommPkgsModel = await _certificateApiService.GetCertificateCommPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);

            if (certificateCommPkgsModel == null)
            {
                var error = $"Certificate {request.ProCoSysGuid} CommPkg scope not found";
                _logger.LogError(error);
                return new NotFoundResult<Unit>(error);
            }

            if (!certificateMcPkgsModel.CertificateIsAccepted && !certificateCommPkgsModel.CertificateIsAccepted)
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. Certificate with guid {request.ProCoSysGuid} in project {request.ProjectName} is not Accepted");
                return new SuccessResult<Unit>(Unit.Value);
            }

            if (!certificateMcPkgsModel.CertificateIsAccepted || !certificateCommPkgsModel.CertificateIsAccepted)
            {
                var error =
                    $"Inconsistent information about acceptance on certificate with guid {request.ProCoSysGuid} in project {request.ProjectName}. " +
                    $"certificateMcPkgsModel.CertificateIsAccepted={certificateMcPkgsModel.CertificateIsAccepted}, " +
                    $"certificateCommPkgsModel.CertificateIsAccepted={certificateCommPkgsModel.CertificateIsAccepted}. This should never happen.";
                _logger.LogError(error);
                return new UnexpectedResult<Unit>(error);
            }

            var commPkgNos = certificateCommPkgsModel.CommPkgs.Select(c => c.CommPkgNo).ToList();
            var mcPkgs = certificateMcPkgsModel.McPkgs.Select(mc =>new Tuple<string, string>(mc.McPkgNo, mc.CommPkgNo)).ToList();


            _invitationRepository.UpdateRfocStatuses(project.Name, commPkgNos, mcPkgs, request.ProCoSysGuid);

            Certificate certificate = null;

            var commPkgs = _invitationRepository.GetCommPkgs(project.Name, commPkgNos);
            if (!commPkgs.IsNullOrEmpty())
            {
                certificate = await GetOrCreateCertificateAsync(request.ProCoSysGuid, project, cancellationToken);
                foreach (var commPkg in commPkgs)
                {
                    certificate.AddCommPkgRelation(commPkg);
                }
            }

            foreach (var mcPkgInfo in mcPkgs)
            {
                var mcPkg = _invitationRepository.GetMcPkg(project.Name, mcPkgInfo.Item2, mcPkgInfo.Item1);
                if (mcPkg != null)
                {
                    certificate ??= await GetOrCreateCertificateAsync(request.ProCoSysGuid, project, cancellationToken);
                    certificate.AddMcPkgRelation(mcPkg);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task<List<McPkg>> GetMcPkgsToUpdateRfocStatusAsync(IList<string> mcPkgNos, Project project)
        {
            var mcPkgList  = new List<McPkg>();
            var pcsMcPkgs = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(project.Plant, project.Name, mcPkgNos);

            foreach (var pcsMcPkg in pcsMcPkgs)
            {
                if (pcsMcPkg.OperationHandoverStatus == "ACCEPTED" && pcsMcPkg.RfocGuid != null && pcsMcPkg.RfocGuid != Guid.Empty)
                {
                    var mcPkgTmp = _invitationRepository.GetMcPkg(project.Name, pcsMcPkg.CommPkgNo, pcsMcPkg.McPkgNo);
                    mcPkgList.Add(mcPkgTmp);
                }
            }

            return mcPkgList;
        }

        private async Task<List<CommPkg>> GetCommPkgsToUpdateRfocStatusAsync(IList<string> commPkgNos, Project project)
        {
            var commPkgList = new List<CommPkg>();
            var pcsCommPkgs = await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(project.Plant, project.Name, commPkgNos);

            foreach (var pcsCommPkg in pcsCommPkgs)
            {
                if (pcsCommPkg.OperationHandoverStatus == "ACCEPTED" && pcsCommPkg.RfocGuid != null && pcsCommPkg.RfocGuid != Guid.Empty)
                {
                    commPkgList.Add(_invitationRepository.GetCommPkg(project.Name, pcsCommPkg.CommPkgNo));
                }
            }

            return commPkgList;
        }

        private async Task<int> HandleCommPkgsAsync(List<Invitation> invitations, Project project, CancellationToken token)
        {
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
                        }
                    }
                }
            }
            return 0;
        }


        private async Task<Certificate> GetOrCreateCertificateAsync(Guid certificateGuid, Project project, CancellationToken cancellationToken)
            => await _certificateRepository.GetCertificateByGuid(certificateGuid) ?? await AddCertificateAsync(certificateGuid, project, cancellationToken);

        private async Task<Certificate> AddCertificateAsync(Guid certificateGuid, Project project, CancellationToken cancellationToken)
        {
            var certificate = new Certificate(project.Plant, project, certificateGuid);
            _certificateRepository.Add(certificate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return certificate;
        }
    }
}
