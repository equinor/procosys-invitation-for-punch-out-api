using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandHandler : IRequestHandler<UpdateAttendedStatusAndNotesOnParticipantsCommand, Result<Unit>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;

        public UpdateAttendedStatusAndNotesOnParticipantsCommandHandler(
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

        public async Task<Result<Unit>> Handle(UpdateAttendedStatusAndNotesOnParticipantsCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var participants = invitation.Participants.Where(p =>
                p.SortKey == 0 &&
                p.Organization == Organization.Contractor &&
                p.AzureOid == currentUserAzureOid).ToList();

            if (!participants.Any() || participants.Any(p => p.FunctionalRoleCode != null))
            {
                var functionalRole = invitation.Participants
                    .Single(p => p.SortKey == 0 &&
                                 p.FunctionalRoleCode != null &&
                                 p.Type == IpoParticipantType.FunctionalRole);

                await UpdateAsPersonInFunctionalRoleAsync(invitation, functionalRole.FunctionalRoleCode, request.Participants);
            }
            else
            {
                UpdateParticipantStatusesAndNotes(invitation, request.Participants);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task UpdateAsPersonInFunctionalRoleAsync(
            Invitation invitation,
            string functionalRoleCode,
            IEnumerable<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                functionalRoleCode);

            if (person != null)
            {
                UpdateParticipantStatusesAndNotes(invitation, participants);
            }
            else
            {
                throw new IpoValidationException($"Person was not found in functional role with code '{functionalRoleCode}'");
            }
        }

        private void UpdateParticipantStatusesAndNotes(
            Invitation invitation,
            IEnumerable<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            var ipoParticipants = invitation.Participants;
            foreach (var participant in participants)
            {
                var ipoParticipant = ipoParticipants.Single(p => p.Id == participant.Id);
                ipoParticipant.Attended = participant.Attended;
                ipoParticipant.Note = participant.Note;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }
    }
}
