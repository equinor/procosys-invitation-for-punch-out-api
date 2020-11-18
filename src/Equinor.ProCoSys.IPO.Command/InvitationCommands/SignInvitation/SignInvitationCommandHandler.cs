using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class SignInvitationCommandHandler : IRequestHandler<SignInvitationCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;

        public SignInvitationCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonApiService personApiService)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
        }

        public async Task<Result<string>> Handle(SignInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var participant = invitation.Participants.Single(p => p.Id == request.ParticipantId);

            if (participant.FunctionalRoleCode != null)
            {
                await SignIpoAsPersonInFunctionalRoleAsync(participant, request.ParticipantRowVersion);
            }
            else
            {
                SignIpoAsPersonAsync(participant, request.ParticipantRowVersion);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(participant.RowVersion.ConvertToString());
        }

        private async Task SignIpoAsPersonInFunctionalRoleAsync(
            Participant participant,
            string participantRowVersion)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);

            if (person != null)
            {
                participant.SignedBy = person.UserName;
                participant.SignedAtUtc = DateTime.UtcNow;
                participant.SetRowVersion(participantRowVersion);
            }
            else
            {
                throw new Exception($"Person was not found in functional role with code '{participant.FunctionalRoleCode}'");
            }
        }

        private void SignIpoAsPersonAsync(
            Participant participant,
            string participantRowVersion)
        {
            participant.SignedBy = participant.UserName;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
        }
    }
}
