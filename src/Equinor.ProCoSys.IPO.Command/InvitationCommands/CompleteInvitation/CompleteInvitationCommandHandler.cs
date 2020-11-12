using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation
{
    public class CompleteInvitationCommandHandler : IRequestHandler<CompleteInvitationCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;

        public CompleteInvitationCommandHandler(
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

        public async Task<Result<string>> Handle(CompleteInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var participants = invitation.Participants.Where(p => 
                p.SortKey == 0 && 
                p.Organization == Organization.Contractor && 
                p.AzureOid == currentUserAzureOid).ToList();

            if (!participants.Any() || participants[0].FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                await CompleteIpoAsPersonInFunctionalRoleAsync(invitation, functionalRole, request.ParticipantRowVersion);
            }
            else
            {
                CompleteIpoAsPersonAsync(invitation, participants.SingleOrDefault(), request.ParticipantRowVersion);
            }

            invitation.SetRowVersion(request.InvitationRowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task CompleteIpoAsPersonInFunctionalRoleAsync(
            Invitation invitation,
            Participant participant,
            string participantRowVersion)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);

            if (person != null)
            {
                invitation.Status = IpoStatus.Completed;
                participant.SignedBy = person.UserName;
                participant.SignedAt = new DateTime();
                participant.SetRowVersion(participantRowVersion);
            }
            else
            {
                throw new Exception($"Person was not found in functional role with code '{participant.FunctionalRoleCode}'");
            }
        }

        private void CompleteIpoAsPersonAsync(
            Invitation invitation,
            Participant participant,
            string participantRowVersion)
        {
            invitation.Status = IpoStatus.Completed;
            participant.SignedBy = participant.UserName;
            participant.SignedAt = new DateTime();
            participant.SetRowVersion(participantRowVersion);
        }
    }
}
