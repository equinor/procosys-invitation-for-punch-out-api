﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
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
        private readonly ILogger<UpdateRfocAcceptedCommandHandler> _logger;

        public UpdateRfocAcceptedCommandHandler(
            IInvitationRepository invitationRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IPlantProvider plantProvider,
            ICertificateApiService certificateApiService,
            ILogger<UpdateRfocAcceptedCommandHandler> logger,
            ICertificateRepository certificateRepository)
        {
            _invitationRepository = invitationRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _certificateApiService = certificateApiService;
            _logger = logger;
            _certificateRepository = certificateRepository;
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
            _logger.LogInformation($"Certificate guid {request.ProCoSysGuid} resulted in certificateAccepted {certificateCommPkgsModel.CertificateIsAccepted}");

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
            _invitationRepository.UpdateRfocStatuses(project.Name, commPkgNos, mcPkgs);
            Certificate certificate = null;

            _logger.LogInformation("Finding commpkgs with nos: " + string.Join(",", commPkgNos));
            var commPkgs = _invitationRepository.GetCommPkgs(project.Name, commPkgNos);
            _logger.LogInformation("CommPkgs found: " + commPkgs.Count);

            if (!commPkgs.IsNullOrEmpty())
            {
                certificate = await GetOrCreateCertificateAsync(request.ProCoSysGuid, project, cancellationToken);
                foreach (var commPkg in commPkgs)
                {
                    _logger.LogInformation("Adding relation with commpkg " + commPkg.CommPkgNo);
                    certificate.AddCommPkgRelation(commPkg);
                }
            }

            foreach (var mcPkgInfo in mcPkgs)
            {
                var mcPkgList = _invitationRepository.GetMcPkgs(project.Name, mcPkgInfo.Item2, mcPkgInfo.Item1);
                if (!mcPkgList.IsNullOrEmpty())
                {
                    certificate ??= await GetOrCreateCertificateAsync(request.ProCoSysGuid, project, cancellationToken);
                    foreach (var mcPkg in mcPkgList)
                    {
                        certificate.AddMcPkgRelation(mcPkg);
                    }
                    
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<Unit>(Unit.Value);
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
