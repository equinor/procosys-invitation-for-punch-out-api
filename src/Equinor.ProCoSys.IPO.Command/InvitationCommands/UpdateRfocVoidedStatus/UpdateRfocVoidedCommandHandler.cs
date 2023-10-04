using System;
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
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocVoidedStatus
{
    public class UpdateRfocVoidedCommandHandler : IRequestHandler<UpdateRfocVoidedCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly ICertificateApiService _certificateApiService;
        private readonly ICertificateRepository _certificateRepository;
        private readonly ILogger<UpdateRfocVoidedCommandHandler> _logger;

        public UpdateRfocVoidedCommandHandler(
            IInvitationRepository invitationRepository,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            IPlantProvider plantProvider,
            ICertificateApiService certificateApiService,
            ICertificateRepository certificateRepository,
            ILogger<UpdateRfocVoidedCommandHandler> logger)
        {
            _invitationRepository = invitationRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _certificateApiService = certificateApiService;
            _certificateRepository = certificateRepository;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateRfocVoidedCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetProjectOnlyByNameAsync(request.ProjectName);
            if (project == null)
            {
                _logger.LogInformation($"Early exit in RfocVoided handling. Project {request.ProjectName} does not exists in IPO module");
                return new SuccessResult<Unit>(Unit.Value);
            }
            
            if (project.IsClosed)
            {
                _logger.LogInformation($"Early exit in RfocVoided handling. Project {request.ProjectName} is closed in IPO module");
                return new SuccessResult<Unit>(Unit.Value);
            }

            var certificateMcPkgsModel = await _certificateApiService.GetCertificateMcPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);

            if (certificateMcPkgsModel != null)
            {
                // Voided certificates are not supposed to be returned from endpoint
                var error = $"Certificate {request.ProCoSysGuid} McPkg is not voided or deleted";
                _logger.LogError(error);
                return new UnexpectedResult<Unit>(error);
            }

            var certificateCommPkgsModel = await _certificateApiService.GetCertificateCommPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);

            if (certificateCommPkgsModel != null)
            {
                // Voided certificates are not supposed to be returned from endpoint
                var error = $"Certificate {request.ProCoSysGuid} CommPkg is not voided or deleted";
                _logger.LogError(error);
                return new UnexpectedResult<Unit>(error);
            }

            var certificate = await _certificateRepository.GetCertificateByGuid(request.ProCoSysGuid);
            if (certificate == null)
            {
                _logger.LogInformation($"Early exit in RfocVoided handling. RFOC {request.ProCoSysGuid} does not exist in IPO module");
                return new SuccessResult<Unit>(Unit.Value);
            }

            _certificateRepository.UpdateRfocStatuses(request.ProCoSysGuid);
            _invitationRepository.ResetScopeHandedOverStatus(request.ProjectName, certificate.CertificateCommPkgs, certificate.CertificateMcPkgs);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
