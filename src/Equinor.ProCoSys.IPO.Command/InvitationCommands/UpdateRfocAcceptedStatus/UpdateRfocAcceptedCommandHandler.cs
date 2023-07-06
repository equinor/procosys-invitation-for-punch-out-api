using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus
{
    public class UpdateRfocAcceptedCommandHandler : IRequestHandler<UpdateRfocAcceptedCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
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
            ILogger<UpdateRfocAcceptedCommandHandler> logger)
        {
            _invitationRepository = invitationRepository;
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _certificateApiService = certificateApiService;
            _logger = logger;
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

            var certificateMcPkgsModel = await _certificateApiService.TryGetCertificateMcPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);
            var certificateCommPkgsModel = await _certificateApiService.TryGetCertificateCommPkgsAsync(_plantProvider.Plant, request.ProCoSysGuid);

            if (certificateMcPkgsModel == null && certificateCommPkgsModel == null)
            {
                var error = $"Certificate {request.ProCoSysGuid} not found";
                _logger.LogError(error);
                return new NotFoundResult<Unit>(error);
            }

            if (!certificateMcPkgsModel.CertificateIsAccepted && !certificateCommPkgsModel.CertificateIsAccepted)
            {
                _logger.LogInformation($"Early exit in RfocAccepted handling. Certificate {request.CertificateNo} of type {request.CertificateType} in project {request.ProjectName} is not Accepted");
                return new SuccessResult<Unit>(Unit.Value);
            }

            var commPkgNos = certificateCommPkgsModel.CommPkgs.Select(c => c.CommPkgNo).ToList();
            var mcPkgs = certificateMcPkgsModel.McPkgs.Select(mc =>new Tuple<string, string>(mc.McPkgNo, mc.CommPkgNo)).ToList();
            _invitationRepository.UpdateRfocStatuses(project.Name, commPkgNos, mcPkgs);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
