using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnCompletePunchOut
{
    public class UnCompletePunchOutCommandHandler : IRequestHandler<UnCompletePunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPermissionCache _permissionCache;

        public UnCompletePunchOutCommandHandler(IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPermissionCache permissionCache)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _permissionCache = permissionCache;
        }

        public async Task<Result<string>> Handle(UnCompletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var hasAdminPrivilege = await InvitationHelper.HasIpoAdminPrivilegeAsync(_permissionCache, _plantProvider, _currentUserProvider);
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 0 && 
                p.Organization == Organization.Contractor &&
                p.SignedAtUtc != null &&
                (p.AzureOid == currentUserAzureOid || hasAdminPrivilege));

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                invitation.UnCompleteIpo(functionalRole, request.ParticipantRowVersion);
            }
            else
            {
                invitation.UnCompleteIpo(participant, request.ParticipantRowVersion);
            }

            invitation.SetRowVersion(request.InvitationRowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }
    }
}
