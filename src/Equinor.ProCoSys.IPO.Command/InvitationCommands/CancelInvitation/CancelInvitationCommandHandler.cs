using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelInvitation
{
    public class CancelInvitationCommandHandler : IRequestHandler<CancelInvitationCommand, Result<object>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CancelInvitationCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<Result<object>> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            invitation.Cancel(currentUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<object>(null);
        }
    }
}
