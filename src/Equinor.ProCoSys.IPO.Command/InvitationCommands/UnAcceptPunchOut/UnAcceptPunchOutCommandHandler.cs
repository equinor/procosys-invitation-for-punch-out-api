using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut
{
    public class UnAcceptPunchOutCommandHandler : IRequestHandler<UnAcceptPunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IMcPkgApiService _mcPkgApiService;

        public UnAcceptPunchOutCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IMcPkgApiService mcPkgApiService)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _mcPkgApiService = mcPkgApiService;
        }

        public async Task<Result<string>> Handle(UnAcceptPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 1 && 
                p.Organization == Organization.ConstructionCompany && 
                p.AzureOid == currentUserAzureOid);

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 1 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                invitation.UnAcceptIpo(functionalRole, request.ParticipantRowVersion);
            }
            else
            {
                invitation.UnAcceptIpo(participant, request.ParticipantRowVersion);
            }

            invitation.SetRowVersion(request.InvitationRowVersion);
            try
            {
                await _mcPkgApiService.ClearM02DatesAsync(
                    _plantProvider.Plant,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception e)
            {
                return new UnexpectedResult<string>("Error: Could not clear M-02 dates");
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }
    }
}
