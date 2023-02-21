using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut
{
    public class SignPunchOutCommandHandler : IRequestHandler<SignPunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonRepository _personRepository;

        public SignPunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonRepository personRepository)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personRepository = personRepository;
        }

        public async Task<Result<string>> Handle(SignPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var participant = invitation.Participants.Single(p => p.Id == request.ParticipantId);
            invitation.SignIpo(participant, currentUser, request.ParticipantRowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(participant.RowVersion.ConvertToString());
        }
    }
}
