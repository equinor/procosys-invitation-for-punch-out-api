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
        private readonly IMcPkgApiForApplicationService _mcPkgApiService;
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
            IMcPkgApiForApplicationService mcPkgApiService,
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

            var certificateMcPkgsModel = await _certificateApiService.TryGetCertificateMcPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid, cancellationToken);

            if (certificateMcPkgsModel == null)
            {
                var error = $"Certificate {request.ProCoSysGuid} McPkg scope not found";
                _logger.LogWarning(error);
                return new NotFoundResult<Unit>(error);
            }

            var certificateCommPkgsModel = await _certificateApiService.TryGetCertificateCommPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid, cancellationToken);

            if (certificateCommPkgsModel == null)
            {
                var error = $"Certificate {request.ProCoSysGuid} CommPkg scope not found";
                _logger.LogWarning(error);
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
            var mcPkgNos = certificateMcPkgsModel.McPkgs.Select(mc => mc.McPkgNo).ToList();

            var pcsMcPkgs = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(project.Plant, project.Name, mcPkgNos, cancellationToken);
            var pcsCommPkgs = await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(project.Plant, project.Name, commPkgNos, cancellationToken);
            if (!pcsMcPkgs.Any() && !pcsCommPkgs.Any())
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. " +
                    $"Certificate with guid {request.ProCoSysGuid} in project {request.ProjectName} does not have scope in IPO.");
                return new SuccessResult<Unit>(Unit.Value);
            }

            var certificate = await _certificateRepository.GetCertificateByGuid(request.ProCoSysGuid);
            if (certificate != null)
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. " +
                                       $"Certificate has already been accepted. ProCoSysGuid:{request.ProCoSysGuid}");
                return new SuccessResult<Unit>(Unit.Value);
            }

            certificate = AddCertificate(request.ProCoSysGuid, project);
            _invitationRepository.RfocAcceptedHandling(
                project.Name,
                pcsCommPkgs.Where(c => c.OperationHandoverStatus == "ACCEPTED").Select(c => c.CommPkgNo).ToList(),
                pcsMcPkgs.Where(mc => mc.OperationHandoverStatus == "ACCEPTED").Select(mc => mc.McPkgNo).ToList());
            AddCertificateMcPkgRelations(pcsMcPkgs.Select(mc => mc.McPkgNo).ToList(), project, certificate);
            AddCertificateCommPkgRelations(pcsCommPkgs.Select(c => c.CommPkgNo).ToList(), project, certificate);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"New certificate created. ProCoSysGuid:{request.ProCoSysGuid}");

            return new SuccessResult<Unit>(Unit.Value);
        }

        private void AddCertificateMcPkgRelations(IList<string> mcPkgNos, Project project, Certificate certificate)
        {
            var mcPkgs = _invitationRepository.GetMcPkgs(project.Name, mcPkgNos);
            if (mcPkgs == null || !mcPkgs.Any())
            {
                return;
            }

            foreach (var mcPkg in mcPkgs)
            {
                certificate.AddMcPkgRelation(mcPkg);
            }
        }

        private void AddCertificateCommPkgRelations(IList<string> commPkgNos, Project project, Certificate certificate)
        {
            var commPkgs = _invitationRepository.GetCommPkgs(project.Name, commPkgNos);
            if (commPkgs == null || !commPkgs.Any())
            {
                return;
            }

            foreach (var commPkg in commPkgs)
            {
                certificate.AddCommPkgRelation(commPkg);
            }
        }

        private Certificate AddCertificate(Guid certificateGuid, Project project)
        {
            var certificate = new Certificate(project.Plant, project, certificateGuid, true);
            _certificateRepository.Add(certificate);
            return certificate;
        }
    }
}
