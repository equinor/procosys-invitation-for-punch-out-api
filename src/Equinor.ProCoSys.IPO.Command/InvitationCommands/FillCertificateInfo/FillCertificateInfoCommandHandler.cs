using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocGuids
{
    public class FillCertificateInfoCommandHandler : IRequestHandler<FillCertificateInfoCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FillCertificateInfoCommandHandler> _logger;

        public FillCertificateInfoCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ILogger<FillCertificateInfoCommandHandler> logger,
            IProjectRepository projectRepository,
            ICertificateRepository certificateRepository)
        {
            _invitationRepository = invitationRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _certificateRepository = certificateRepository;
        }

        public async Task<Result<Unit>> Handle(FillCertificateInfoCommand request, CancellationToken cancellationToken)
        {
            // THIS CODE WAS WRITTEN TO RUN A ONETIME TRANSFORMATION WHEN WE INTRODUCED Certificate table to move data from mcpkg/commpkg to certificate
            // WE KEEP THE CODE ... MAYBE WE WANT TO DO SIMILAR STUFF LATER

            var projects = await _projectRepository.GetAllAsync();
            var invitations = _invitationRepository.GetInvitationsForSynchronization();

            var certificatesUpdatedCount = 0;

            foreach (var project in projects)
            {
                var invitationsInProject = invitations.Where(i => i.ProjectId == project.Id).ToList();

                foreach (var invitation in invitationsInProject)
                {
                    foreach(var mcPkg in invitation.McPkgs.Where(mc => mc.RfocAccepted))
                    {
                        foreach (var certificateScope in mcPkg.CertificateScopes)
                        {
                            var certificate = await _certificateRepository.GetCertificateByGuid(certificateScope.PcsGuid);
                            if (certificate == null)
                            {
                                throw new Exception($"Expected to find certificate {certificateScope.PcsGuid} that was on mc pkg {mcPkg.McPkgNo}");
                            }
                            if (!certificate.IsAccepted)
                            {
                                certificate.SetIsAccepted();
                                certificatesUpdatedCount++;
                            }
                        }
                    }
                    foreach (var commPkg in invitation.CommPkgs.Where(mc => mc.RfocAccepted))
                    {
                        foreach (var certificateScope in commPkg.CertificateScopes)
                        {
                            var certificate = await _certificateRepository.GetCertificateByGuid(certificateScope.PcsGuid);
                            if (certificate == null)
                            {
                                throw new Exception($"Expected to find certificate {certificateScope.PcsGuid} that was on comm pkg {commPkg.CommPkgNo}");
                            }
                            if (!certificate.IsAccepted)
                            {
                                certificate.SetIsAccepted();
                                certificatesUpdatedCount++;
                            }
                        }
                    }
                }

                _logger.LogInformation($"FillRfocInfo: Project updated: {project.Name}");
            }

            if (certificatesUpdatedCount > 0)
            {
                if (request.SaveChanges)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                _logger.LogInformation($"Certificates updated: {certificatesUpdatedCount}");
            }

            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
