using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut
{
    public class AcceptPunchOutCommandHandler : IRequestHandler<AcceptPunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonRepository _personRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public AcceptPunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPersonRepository personRepository,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personRepository = personRepository;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result<string>> Handle(AcceptPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 1 && 
                p.Organization == Organization.ConstructionCompany && 
                p.AzureOid == currentUser.Guid);
            var acceptedAtUtc = DateTime.UtcNow;

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 1 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);
                invitation.AcceptIpo(functionalRole, request.ParticipantRowVersion, currentUser, acceptedAtUtc);
            }
            else
            {
                invitation.AcceptIpo(participant, request.ParticipantRowVersion, currentUser, acceptedAtUtc);
            }
            UpdateNotesOnParticipants(invitation, request.Participants);

            invitation.SetRowVersion(request.InvitationRowVersion);
            
            await PublishEventToBusAsync(cancellationToken, invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Invitation invitation)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = invitation.Plant,
                Event = "Accepted",
                InvitationGuid = invitation.Guid
            };

            await _integrationEventPublisher.PublishAsync(eventMessage, cancellationToken);
        }


        private void UpdateNotesOnParticipants(Invitation invitation, IList<UpdateNoteOnParticipantForCommand> participants)
        {
            foreach (var participant in participants)
            {
                var ipoParticipant = invitation.Participants.Single(p => p.Id == participant.Id);
                ipoParticipant.Note = participant.Note;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }
    }
}
