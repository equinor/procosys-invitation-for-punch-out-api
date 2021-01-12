using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut
{
    public class SignPunchOutCommandHandler : IRequestHandler<SignPunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;
        private readonly IPersonRepository _personRepository;

        public SignPunchOutCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonApiService personApiService,
            IPersonRepository personRepository)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
            _personRepository = personRepository;
        }

        public async Task<Result<string>> Handle(SignPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var participant = invitation.Participants.Single(p => p.Id == request.ParticipantId);

            if (participant.FunctionalRoleCode != null)
            {
                await SignIpoAsPersonInFunctionalRoleAsync(invitation, participant, currentUser, request.ParticipantRowVersion);
            }
            else
            {
                invitation.SignIpo(participant, currentUser, request.ParticipantRowVersion);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(participant.RowVersion.ConvertToString());
        }

        private async Task SignIpoAsPersonInFunctionalRoleAsync(
            Invitation invitation,
            Participant participant,
            Person currentUser,
            string participantRowVersion)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);

            if (person != null)
            {
                invitation.SignIpo(participant, currentUser, participantRowVersion);
            }
            else
            {
                throw new Exception($"Person was not found in functional role with code '{participant.FunctionalRoleCode}'");
            }
        }
    }
}
