using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut
{
    public class CompletePunchOutCommandHandler : IRequestHandler<CompletePunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonRepository _personRepository;

        public CompletePunchOutCommandHandler(IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPersonRepository personRepository)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personRepository = personRepository;
        }

        public async Task<Result<string>> Handle(CompletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var completedAtUtc = DateTime.UtcNow;
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 0 && 
                p.Organization == Organization.Contractor && 
                p.AzureOid == currentUser.Oid);

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                invitation.CompleteIpo(functionalRole, request.ParticipantRowVersion, currentUser, completedAtUtc);
            }
            else
            {
                invitation.CompleteIpo(participant, request.ParticipantRowVersion, currentUser, completedAtUtc);
            }
            UpdateAttendedStatusAndNotesOnParticipants(invitation, request.Participants);
            invitation.SetRowVersion(request.InvitationRowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private void UpdateAttendedStatusAndNotesOnParticipants(Invitation invitation,
            IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            foreach (var participant in participants)
            {
                var ipoParticipant = invitation.Participants.Single(p => p.Id == participant.Id);
                ipoParticipant.Note = participant.Note;
                ipoParticipant.Attended = participant.Attended;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }
    }
}
