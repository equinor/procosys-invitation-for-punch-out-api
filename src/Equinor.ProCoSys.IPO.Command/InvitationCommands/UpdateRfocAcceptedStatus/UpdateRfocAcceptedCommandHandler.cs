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
            var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

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
            var mcPkgNos = certificateMcPkgsModel.McPkgs.Select(mc => mc.McPkgNo).ToList();

            var mcPkgNosToUpdateStatusOn = await GetMcPkgNosToUpdateRfocStatusAsync(mcPkgNos, project);
            var commPkgNosToUpdateStatusOn = await GetCommPkgNosToUpdateRfocStatusAsync(commPkgNos, project);
            _invitationRepository.UpdateRfocStatuses(project.Name, commPkgNosToUpdateStatusOn, mcPkgNosToUpdateStatusOn);

            try
            {
                await AddCertificateMcPkgRelationsAsync(request.ProCoSysGuid, mcPkgNos, project, cancellationToken);
                await AddCertificateCommPkgRelationsAsync(request.ProCoSysGuid, commPkgNos, project, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _unitOfWork.Commit();

            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task AddCertificateMcPkgRelationsAsync(Guid certificateGuid, IList<string> mcPkgNos, Project project, CancellationToken cancellationToken)
        {
            var mcPkgs = _invitationRepository.GetMcPkgs(project.Name, mcPkgNos);
            if (!mcPkgs.IsNullOrEmpty())
            {
                var certificate = await GetOrCreateCertificateAsync(certificateGuid, project, cancellationToken);
                foreach (var mcPkg in mcPkgs)
                {
                    certificate.AddMcPkgRelation(mcPkg);
                }
            }
        }

        private async Task AddCertificateCommPkgRelationsAsync(Guid certificateGuid, IList<string> commPkgNos, Project project, CancellationToken cancellationToken)
        {
            var commPkgs = _invitationRepository.GetCommPkgs(project.Name, commPkgNos);
            if (!commPkgs.IsNullOrEmpty())
            {
                var certificate = await GetOrCreateCertificateAsync(certificateGuid, project, cancellationToken);
                foreach (var commPkg in commPkgs)
                {
                    certificate.AddCommPkgRelation(commPkg);
                }
            }
        }

        private async Task<IList<string>> GetMcPkgNosToUpdateRfocStatusAsync(IList<string> mcPkgNos, Project project)
        {
            var pcsMcPkgs = await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(project.Plant, project.Name, mcPkgNos);
            return pcsMcPkgs.Where(mc => mc.OperationHandoverStatus == "ACCEPTED").Select(mc => mc.McPkgNo).ToList();
        }

        private async Task<IList<string>> GetCommPkgNosToUpdateRfocStatusAsync(IList<string> commPkgNos, Project project)
        {
            var pcsCommPkgs = await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(project.Plant, project.Name, commPkgNos);
            return pcsCommPkgs.Where(c => c.OperationHandoverStatus == "ACCEPTED").Select(c => c.CommPkgNo).ToList();
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
