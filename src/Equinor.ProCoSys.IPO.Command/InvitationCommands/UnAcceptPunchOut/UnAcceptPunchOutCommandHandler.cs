using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut
{
    public class UnAcceptPunchOutCommandHandler : IRequestHandler<UnAcceptPunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPlantProvider _plantProvider;
        private readonly IPermissionCache _permissionCache;

        public UnAcceptPunchOutCommandHandler(IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPlantProvider plantProvider,
            IPermissionCache permissionCache)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _plantProvider = plantProvider;
            _permissionCache = permissionCache;
        }

        public async Task<Result<string>> Handle(UnAcceptPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var hasAdminPrivilege = await InvitationHelper.HasIpoAdminPrivilege(_permissionCache, _plantProvider.Plant, _currentUserProvider.GetCurrentUserOid());
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 1 && 
                p.Organization == Organization.ConstructionCompany && 
                (p.AzureOid == currentUserAzureOid || hasAdminPrivilege));

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
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }
    }
}
